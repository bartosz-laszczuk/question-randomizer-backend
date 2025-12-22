namespace QuestionRandomizer.Modules.Randomization.Application.Commands.SelectedCategories.AddSelectedCategory;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;

/// <summary>
/// Command to add a category to the selected categories for a randomization session
/// </summary>
public record AddSelectedCategoryCommand : IRequest<SelectedCategoryDto>
{
    public string RandomizationId { get; init; } = string.Empty;
    public string CategoryId { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
}
