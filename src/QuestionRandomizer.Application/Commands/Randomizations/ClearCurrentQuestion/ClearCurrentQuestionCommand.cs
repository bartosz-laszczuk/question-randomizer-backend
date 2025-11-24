namespace QuestionRandomizer.Application.Commands.Randomizations.ClearCurrentQuestion;

using MediatR;

/// <summary>
/// Command to clear the current question from a randomization session
/// </summary>
public record ClearCurrentQuestionCommand : IRequest<Unit>
{
    public string RandomizationId { get; init; } = string.Empty;
}
