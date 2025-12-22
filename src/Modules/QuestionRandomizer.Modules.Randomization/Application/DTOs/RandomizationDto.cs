namespace QuestionRandomizer.Modules.Randomization.Application.DTOs;

/// <summary>
/// Data transfer object for Randomization
/// </summary>
public record RandomizationDto
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public bool ShowAnswer { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CurrentQuestionId { get; init; }
    public DateTime? CreatedAt { get; init; }
}
