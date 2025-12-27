namespace QuestionRandomizer.Modules.Agent.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Infrastructure.Queue;

/// <summary>
/// Service for executing agent tasks using the integrated .NET AI agent
/// Replaced the HTTP client to TypeScript agent service
/// </summary>
public class AgentService : IAgentService
{
    private readonly IAgentExecutor _agentExecutor;
    private readonly ITaskQueueService _taskQueueService;
    private readonly ILogger<AgentService> _logger;

    // In-memory task status tracking (simplified for now; could use cache/database in production)
    private readonly Dictionary<string, AgentTaskStatus> _taskStatuses = new();

    public AgentService(
        IAgentExecutor agentExecutor,
        ITaskQueueService taskQueueService,
        ILogger<AgentService> logger)
    {
        _agentExecutor = agentExecutor;
        _taskQueueService = taskQueueService;
        _logger = logger;
    }

    public async Task<AgentTaskResult> ExecuteTaskAsync(
        string task,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing agent task for user {UserId}", userId);

        var result = await _agentExecutor.ExecuteTaskAsync(task, userId, cancellationToken);

        // Store task status
        _taskStatuses[result.TaskId] = new AgentTaskStatus
        {
            TaskId = result.TaskId,
            Status = result.Success ? "completed" : "failed",
            Result = result.Result,
            Error = result.Error,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Agent task {TaskId} {Status}",
            result.TaskId,
            result.Success ? "completed successfully" : "failed");

        return result;
    }

    public Task<AgentTaskStatus> GetTaskStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting status for task {TaskId}", taskId);

        if (_taskStatuses.TryGetValue(taskId, out var status))
        {
            return Task.FromResult(status);
        }

        // Task not found - return unknown status
        return Task.FromResult(new AgentTaskStatus
        {
            TaskId = taskId,
            Status = "unknown",
            Error = "Task not found"
        });
    }

    public async Task<AgentTaskResult> ExecuteTaskStreamingAsync(
        string task,
        string userId,
        Action<AgentStreamEvent> onProgress,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing streaming agent task for user {UserId}", userId);

        var finalResult = new AgentTaskResult
        {
            TaskId = Guid.NewGuid().ToString(),
            Success = true,
            Result = string.Empty,
            Error = null
        };

        try
        {
            // Execute task with streaming
            await foreach (var streamEvent in _agentExecutor.ExecuteTaskStreamingAsync(task, userId, cancellationToken))
            {
                // Forward progress events to caller
                onProgress(streamEvent);

                // Capture final result
                if (streamEvent.Type == "completed" && !string.IsNullOrEmpty(streamEvent.Content))
                {
                    finalResult = finalResult with
                    {
                        Success = true,
                        Result = streamEvent.Content
                    };
                }
                else if (streamEvent.Type == "error")
                {
                    finalResult = finalResult with
                    {
                        Success = false,
                        Error = streamEvent.Message ?? streamEvent.Output
                    };
                }
            }

            // Store task status
            _taskStatuses[finalResult.TaskId] = new AgentTaskStatus
            {
                TaskId = finalResult.TaskId,
                Status = finalResult.Success ? "completed" : "failed",
                Result = finalResult.Result,
                Error = finalResult.Error,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Streaming agent task completed for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during streaming agent task execution");
            finalResult = finalResult with
            {
                Success = false,
                Error = $"Unexpected error: {ex.Message}"
            };
        }

        return finalResult;
    }

    public async Task<string> QueueTaskAsync(
        string task,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Queueing agent task for user {UserId}", userId);

        var taskId = await _taskQueueService.QueueTaskAsync(task, userId, cancellationToken);

        // Store initial task status
        _taskStatuses[taskId] = new AgentTaskStatus
        {
            TaskId = taskId,
            Status = "queued",
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Agent task queued with ID {TaskId}", taskId);

        return taskId;
    }
}
