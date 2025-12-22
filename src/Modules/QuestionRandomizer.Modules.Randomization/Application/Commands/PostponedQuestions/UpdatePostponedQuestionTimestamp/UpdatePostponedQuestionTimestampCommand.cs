namespace QuestionRandomizer.Modules.Randomization.Application.Commands.PostponedQuestions.UpdatePostponedQuestionTimestamp;

using MediatR;

/// <summary>
/// Command to update the timestamp of a postponed question
/// </summary>
public record UpdatePostponedQuestionTimestampCommand : IRequest<Unit>
{
    public string RandomizationId { get; init; } = string.Empty;
    public string QuestionId { get; init; } = string.Empty;
}
