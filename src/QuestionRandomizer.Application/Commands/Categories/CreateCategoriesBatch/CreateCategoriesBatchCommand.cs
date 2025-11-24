namespace QuestionRandomizer.Application.Commands.Categories.CreateCategoriesBatch;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create multiple categories at once
/// </summary>
public record CreateCategoriesBatchCommand : IRequest<List<CategoryDto>>
{
    public List<string> CategoryNames { get; init; } = new();
}
