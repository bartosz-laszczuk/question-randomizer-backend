namespace QuestionRandomizer.Modules.Questions.Application.Commands.Questions.RemoveCategoryFromQuestions;

using MediatR;

/// <summary>
/// Command to remove a category ID from all questions
/// </summary>
public record RemoveCategoryFromQuestionsCommand : IRequest<Unit>
{
    public string CategoryId { get; init; } = string.Empty;
}
