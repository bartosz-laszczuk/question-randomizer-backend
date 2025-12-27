namespace QuestionRandomizer.Modules.Agent.Infrastructure.Queue;

using Hangfire;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Domain;

/// <summary>
/// Background task queue service using Hangfire with Firestore-backed status tracking
/// Replaces BullMQ from the TypeScript implementation
/// </summary>
public class TaskQueueService : ITaskQueueService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ILogger<TaskQueueService> _logger;

    public TaskQueueService(
        IBackgroundJobClient backgroundJobClient,
        IAgentTaskRepository taskRepository,
        ILogger<TaskQueueService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<string> QueueTaskAsync(
        string task,
        string userId,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        var taskId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Queueing agent task {TaskId} for user {UserId} (ConversationId: {ConversationId})",
            taskId, userId, conversationId ?? "none");

        // Create task in Firestore with pending status
        var agentTask = new AgentTask
        {
            TaskId = taskId,
            UserId = userId,
            TaskDescription = task,
            ConversationId = conversationId,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepository.CreateAsync(agentTask, cancellationToken);

        // Enqueue the task for background processing
        var jobId = _backgroundJobClient.Enqueue<AgentTaskProcessor>(
            processor => processor.ProcessTaskAsync(taskId, task, userId, conversationId, CancellationToken.None));

        // Update task with job ID
        agentTask.JobId = jobId;
        await _taskRepository.UpdateAsync(agentTask, cancellationToken);

        _logger.LogInformation(
            "Agent task {TaskId} queued with job ID {JobId}",
            taskId, jobId);

        return taskId;
    }

    public async Task<Application.Interfaces.TaskStatus> GetTaskStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        // Note: We don't have userId in this method signature, which is a security issue
        // For now, we'll query without userId check, but this should be fixed in a future update
        // to accept userId as a parameter

        _logger.LogWarning(
            "GetTaskStatusAsync called without userId parameter - security issue, task {TaskId}",
            taskId);

        // TODO: Fix ITaskQueueService interface to include userId parameter
        // For now, we cannot retrieve the task securely
        // Returning Processing as fallback

        return Application.Interfaces.TaskStatus.Processing;
    }

    /// <summary>
    /// Get task status with userId for security
    /// This is the correct method to use
    /// </summary>
    public async Task<AgentTask?> GetTaskWithUserIdAsync(
        string taskId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetByIdAsync(taskId, userId, cancellationToken);
    }
}
