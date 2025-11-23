namespace QuestionRandomizer.Application.DTOs;

/// <summary>
/// Data transfer object for Conversation
/// </summary>
public record ConversationDto
{
    public string Id { get; init; } = string.Empty;
    public string? Title { get; init; }
    public bool IsActive { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
