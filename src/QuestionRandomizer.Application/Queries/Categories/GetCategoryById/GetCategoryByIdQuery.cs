namespace QuestionRandomizer.Application.Queries.Categories.GetCategoryById;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get a category by ID
/// </summary>
public record GetCategoryByIdQuery : IRequest<CategoryDto>
{
    public string Id { get; init; } = string.Empty;
}
