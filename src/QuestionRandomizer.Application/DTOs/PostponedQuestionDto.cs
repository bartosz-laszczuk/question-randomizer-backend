namespace QuestionRandomizer.Application.DTOs;

/// <summary>
/// DTO for postponed question in a randomization session
/// </summary>
public record PostponedQuestionDto
{
    public string Id { get; init; } = string.Empty;
    public string QuestionId { get; init; } = string.Empty;
    public DateTime? Timestamp { get; init; }
}
