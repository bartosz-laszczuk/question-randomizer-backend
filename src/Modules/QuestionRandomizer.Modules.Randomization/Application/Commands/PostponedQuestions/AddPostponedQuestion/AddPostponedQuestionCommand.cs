namespace QuestionRandomizer.Modules.Randomization.Application.Commands.PostponedQuestions.AddPostponedQuestion;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;

/// <summary>
/// Command to add a question to the postponed questions list
/// </summary>
public record AddPostponedQuestionCommand : IRequest<PostponedQuestionDto>
{
    public string RandomizationId { get; init; } = string.Empty;
    public string QuestionId { get; init; } = string.Empty;
}
