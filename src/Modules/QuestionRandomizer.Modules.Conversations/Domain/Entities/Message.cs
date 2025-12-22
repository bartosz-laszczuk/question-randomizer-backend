namespace QuestionRandomizer.Modules.Conversations.Domain.Entities;

/// <summary>
/// Represents a message within a conversation
/// </summary>
public class Message
{
    /// <summary>
    /// Unique identifier for the message (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the conversation this message belongs to
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// The role of the message sender (user or assistant)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The content of the message
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the message was sent (managed by Firestore)
    /// </summary>
    public DateTime? Timestamp { get; set; }
}
