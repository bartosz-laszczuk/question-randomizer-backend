namespace QuestionRandomizer.Modules.Agent.Application.Interfaces;

/// <summary>
/// Service for queueing agent tasks for background processing
/// Replaces BullMQ from the TypeScript implementation
/// </summary>
public interface ITaskQueueService
{
    /// <summary>
    /// Queues an agent task for background execution
    /// </summary>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">The user ID making the request</param>
    /// <param name="conversationId">Optional conversation ID for context continuity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for tracking</returns>
    Task<string> QueueTaskAsync(
        string task,
        string userId,
        string? conversationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a queued task
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task status information</returns>
    Task<TaskStatus> GetTaskStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Status of a queued task
/// </summary>
public enum TaskStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}
