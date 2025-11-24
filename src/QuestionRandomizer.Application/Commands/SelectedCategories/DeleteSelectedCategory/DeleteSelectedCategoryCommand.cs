namespace QuestionRandomizer.Application.Commands.SelectedCategories.DeleteSelectedCategory;

using MediatR;

/// <summary>
/// Command to remove a category from the selected categories list
/// </summary>
public record DeleteSelectedCategoryCommand : IRequest<Unit>
{
    public string RandomizationId { get; init; } = string.Empty;
    public string CategoryId { get; init; } = string.Empty;
}
