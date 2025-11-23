namespace QuestionRandomizer.Application.DTOs;

/// <summary>
/// Data transfer object for Question
/// </summary>
public record QuestionDto
{
    public string Id { get; init; } = string.Empty;
    public string QuestionText { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public string AnswerPl { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? QualificationId { get; init; }
    public string? QualificationName { get; init; }
    public bool IsActive { get; init; }
    public List<string>? Tags { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
