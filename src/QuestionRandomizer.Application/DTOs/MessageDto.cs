namespace QuestionRandomizer.Application.DTOs;

/// <summary>
/// Data transfer object for Message
/// </summary>
public record MessageDto
{
    public string Id { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime? Timestamp { get; init; }
}
