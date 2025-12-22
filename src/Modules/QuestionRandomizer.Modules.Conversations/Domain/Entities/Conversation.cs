namespace QuestionRandomizer.Modules.Conversations.Domain.Entities;

/// <summary>
/// Represents a conversation between a user and the AI agent
/// </summary>
public class Conversation
{
    /// <summary>
    /// Unique identifier for the conversation (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Optional title for the conversation
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// ID of the user who owns this conversation
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the conversation is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the conversation was created (managed by Firestore)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the conversation was last updated (managed by Firestore)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
