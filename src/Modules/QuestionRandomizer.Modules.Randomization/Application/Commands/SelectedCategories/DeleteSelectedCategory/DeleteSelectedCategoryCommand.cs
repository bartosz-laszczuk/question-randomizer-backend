namespace QuestionRandomizer.Modules.Randomization.Application.Commands.SelectedCategories.DeleteSelectedCategory;

using MediatR;

/// <summary>
/// Command to remove a category from the selected categories for a randomization session
/// </summary>
public record DeleteSelectedCategoryCommand : IRequest<bool>
{
    public string RandomizationId { get; init; } = string.Empty;
    public string CategoryId { get; init; } = string.Empty;
}
