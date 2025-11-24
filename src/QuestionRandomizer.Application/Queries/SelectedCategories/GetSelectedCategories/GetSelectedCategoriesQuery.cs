namespace QuestionRandomizer.Application.Queries.SelectedCategories.GetSelectedCategories;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get all selected categories for a randomization session
/// </summary>
public record GetSelectedCategoriesQuery : IRequest<List<SelectedCategoryDto>>
{
    public string RandomizationId { get; init; } = string.Empty;
}
