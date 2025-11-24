namespace QuestionRandomizer.Application.Commands.UsedQuestions.DeleteUsedQuestion;

using MediatR;

/// <summary>
/// Command to remove a question from the used questions list
/// </summary>
public record DeleteUsedQuestionCommand : IRequest<Unit>
{
    public string RandomizationId { get; init; } = string.Empty;
    public string QuestionId { get; init; } = string.Empty;
}
