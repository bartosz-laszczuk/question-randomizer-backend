namespace QuestionRandomizer.Application.Commands.Categories.UpdateCategory;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to update an existing category
/// </summary>
public record UpdateCategoryCommand : IRequest<CategoryDto>
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
