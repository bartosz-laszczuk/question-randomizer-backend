namespace QuestionRandomizer.Modules.Questions.Application.Queries.Categories.GetCategories;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Query to get all categories for the current user
/// </summary>
public record GetCategoriesQuery : IRequest<List<CategoryDto>>
{
}
