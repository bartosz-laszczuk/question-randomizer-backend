namespace QuestionRandomizer.Modules.Questions.Application.Commands.Categories.UpdateCategory;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Command to update an existing category
/// </summary>
public record UpdateCategoryCommand : IRequest<CategoryDto>
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
