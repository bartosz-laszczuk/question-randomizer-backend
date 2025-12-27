namespace QuestionRandomizer.Modules.Agent.Domain;

/// <summary>
/// Domain entity for agent task tracking in Firestore
/// Collection: agent_tasks
/// </summary>
public class AgentTask
{
    /// <summary>
    /// Unique task identifier
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// User who initiated the task
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The task description/prompt given to the agent
    /// </summary>
    public string TaskDescription { get; set; } = string.Empty;

    /// <summary>
    /// Current status: pending, processing, completed, failed
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Task result when completed
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Hangfire job ID for background tasks
    /// </summary>
    public string? JobId { get; set; }

    /// <summary>
    /// When the task was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the task started processing
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// When the task completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Additional metadata (tool calls, iterations, etc.)
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
