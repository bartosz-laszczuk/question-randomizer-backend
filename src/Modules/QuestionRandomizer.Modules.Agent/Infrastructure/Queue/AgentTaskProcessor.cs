namespace QuestionRandomizer.Modules.Agent.Infrastructure.Queue;

using Hangfire;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;

/// <summary>
/// Background processor for agent tasks with Firestore-backed status tracking
/// Executed by Hangfire workers
/// </summary>
public class AgentTaskProcessor
{
    private readonly IAgentExecutor _agentExecutor;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ILogger<AgentTaskProcessor> _logger;

    public AgentTaskProcessor(
        IAgentExecutor agentExecutor,
        IAgentTaskRepository taskRepository,
        ILogger<AgentTaskProcessor> logger)
    {
        _agentExecutor = agentExecutor;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    /// <summary>
    /// Processes an agent task in the background with status updates
    /// This method is called by Hangfire
    /// </summary>
    /// <param name="taskId">Unique task identifier</param>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 5, 15, 30 })]
    public async Task ProcessTaskAsync(
        string taskId,
        string task,
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing background agent task {TaskId} for user {UserId}",
                taskId, userId);

            // Update status to processing
            await _taskRepository.UpdateStatusAsync(
                taskId,
                userId,
                "processing",
                DateTime.UtcNow,
                cancellationToken);

            // Execute the agent task
            var result = await _agentExecutor.ExecuteTaskAsync(task, userId, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Background agent task {TaskId} completed successfully",
                    taskId);

                // Store result in Firestore
                await _taskRepository.SetResultAsync(taskId, userId, result.Result, cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "Background agent task {TaskId} failed: {Error}",
                    taskId, result.Error);

                // Store error in Firestore
                await _taskRepository.SetErrorAsync(
                    taskId,
                    userId,
                    result.Error ?? "Unknown error",
                    cancellationToken);

                throw new InvalidOperationException(
                    $"Agent task failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing background agent task {TaskId}",
                taskId);

            // Store error in Firestore
            try
            {
                await _taskRepository.SetErrorAsync(
                    taskId,
                    userId,
                    ex.Message,
                    cancellationToken);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(
                    innerEx,
                    "Failed to store error status for task {TaskId}",
                    taskId);
            }

            // Hangfire will retry based on [AutomaticRetry] attribute
            throw;
        }
    }
}
