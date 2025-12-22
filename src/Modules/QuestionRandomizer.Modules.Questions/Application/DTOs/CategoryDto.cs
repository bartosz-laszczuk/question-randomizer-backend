namespace QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Data transfer object for Category
/// </summary>
public record CategoryDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
