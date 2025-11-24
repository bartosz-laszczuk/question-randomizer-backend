namespace QuestionRandomizer.Application.Queries.Categories.GetCategories;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get all categories for the current user
/// </summary>
public record GetCategoriesQuery : IRequest<List<CategoryDto>>
{
}
