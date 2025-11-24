namespace QuestionRandomizer.Application.DTOs;

/// <summary>
/// DTO for used question in a randomization session
/// </summary>
public record UsedQuestionDto
{
    public string Id { get; init; } = string.Empty;
    public string QuestionId { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public DateTime? CreatedAt { get; init; }
}
