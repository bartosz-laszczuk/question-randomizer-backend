namespace QuestionRandomizer.Modules.Questions.Application.Commands.Categories.CreateCategoriesBatch;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Command to create multiple categories at once
/// </summary>
public record CreateCategoriesBatchCommand : IRequest<List<CategoryDto>>
{
    public List<string> CategoryNames { get; init; } = new();
}
