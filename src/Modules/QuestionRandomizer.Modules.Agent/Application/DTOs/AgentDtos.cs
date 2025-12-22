namespace QuestionRandomizer.Modules.Agent.Application.DTOs;

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
