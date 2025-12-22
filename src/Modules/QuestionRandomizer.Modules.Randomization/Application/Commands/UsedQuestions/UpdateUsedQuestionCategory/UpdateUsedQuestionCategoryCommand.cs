namespace QuestionRandomizer.Modules.Randomization.Application.Commands.UsedQuestions.UpdateUsedQuestionCategory;

using MediatR;

/// <summary>
/// Command to update category information for used questions
/// </summary>
public record UpdateUsedQuestionCategoryCommand : IRequest<Unit>
{
    public string RandomizationId { get; init; } = string.Empty;
    public string QuestionId { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? CategoryName { get; init; }
}
