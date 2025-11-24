namespace QuestionRandomizer.Application.Commands.Categories.CreateCategory;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create a new category
/// </summary>
public record CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; init; } = string.Empty;
}
