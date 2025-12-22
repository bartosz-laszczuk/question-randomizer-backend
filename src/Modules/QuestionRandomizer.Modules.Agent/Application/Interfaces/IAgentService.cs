namespace QuestionRandomizer.Modules.Agent.Application.Interfaces;

using QuestionRandomizer.Modules.Agent.Application.DTOs;

/// <summary>
/// Service for executing AI agent tasks
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Executes an agent task synchronously (waits for completion)
    /// </summary>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">The user ID making the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the agent task execution</returns>
    Task<AgentTaskResult> ExecuteTaskAsync(string task, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an agent task with streaming progress updates (Server-Sent Events)
    /// </summary>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">The user ID making the request</param>
    /// <param name="onProgress">Callback for progress updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the agent task execution</returns>
    Task<AgentTaskResult> ExecuteTaskStreamingAsync(
        string task,
        string userId,
        Action<AgentStreamEvent> onProgress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queues an agent task for background processing
    /// </summary>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">The user ID making the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for tracking</returns>
    Task<string> QueueTaskAsync(string task, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a queued agent task
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status of the task</returns>
    Task<AgentTaskStatus> GetTaskStatusAsync(string taskId, CancellationToken cancellationToken = default);
}
