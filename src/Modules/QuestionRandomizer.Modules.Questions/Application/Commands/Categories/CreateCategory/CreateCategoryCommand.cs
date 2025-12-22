namespace QuestionRandomizer.Modules.Questions.Application.Commands.Categories.CreateCategory;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Command to create a new category
/// </summary>
public record CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; init; } = string.Empty;
}
