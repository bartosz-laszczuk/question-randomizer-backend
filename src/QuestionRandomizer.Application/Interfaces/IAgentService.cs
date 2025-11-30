namespace QuestionRandomizer.Application.Interfaces;

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
    public DateTime? CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public AgentTaskMetadata? Metadata { get; init; }
}

/// <summary>
/// Metadata about agent task execution
/// </summary>
public record AgentTaskMetadata
{
    public int ToolsUsed { get; init; }
    public int Iterations { get; init; }
    public int DurationMs { get; init; }
    public TokenUsage? TokensUsed { get; init; }
}

/// <summary>
/// Token usage information
/// </summary>
public record TokenUsage
{
    public int Input { get; init; }
    public int Output { get; init; }
    public int Total { get; init; }
}

/// <summary>
/// Streaming event from agent execution
/// </summary>
public record AgentStreamEvent
{
    public string Type { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string? ToolName { get; init; }
    public object? Input { get; init; }
    public string? Output { get; init; }
    public string? Content { get; init; }
    public int? Progress { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
