namespace QuestionRandomizer.Application.Interfaces;

/// <summary>
/// Service for executing AI agent tasks
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Executes an agent task asynchronously
    /// </summary>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">The user ID making the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the agent task execution</returns>
    Task<AgentTaskResult> ExecuteTaskAsync(string task, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of an agent task
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status of the task</returns>
    Task<AgentTaskStatus> GetTaskStatusAsync(string taskId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an agent task execution
/// </summary>
public record AgentTaskResult
{
    public string TaskId { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>
/// Status of an agent task
/// </summary>
public record AgentTaskStatus
{
    public string TaskId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Result { get; init; }
    public string? Error { get; init; }
}
