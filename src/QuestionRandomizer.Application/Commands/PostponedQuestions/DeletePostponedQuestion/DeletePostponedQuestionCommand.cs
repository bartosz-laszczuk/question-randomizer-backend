namespace QuestionRandomizer.Application.Commands.PostponedQuestions.DeletePostponedQuestion;

using MediatR;

/// <summary>
/// Command to remove a question from the postponed questions list
/// </summary>
public record DeletePostponedQuestionCommand : IRequest<Unit>
{
    public string RandomizationId { get; init; } = string.Empty;
    public string QuestionId { get; init; } = string.Empty;
}
