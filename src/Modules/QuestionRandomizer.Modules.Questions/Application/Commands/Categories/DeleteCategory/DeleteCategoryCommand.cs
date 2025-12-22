namespace QuestionRandomizer.Modules.Questions.Application.Commands.Categories.DeleteCategory;

using MediatR;

/// <summary>
/// Command to delete a category
/// </summary>
public record DeleteCategoryCommand : IRequest<Unit>
{
    public string Id { get; init; } = string.Empty;
}
