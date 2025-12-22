namespace QuestionRandomizer.Modules.Randomization.Application.DTOs;

/// <summary>
/// DTO for selected category in a randomization session
/// </summary>
public record SelectedCategoryDto
{
    public string Id { get; init; } = string.Empty;
    public string CategoryId { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public DateTime? CreatedAt { get; init; }
}
