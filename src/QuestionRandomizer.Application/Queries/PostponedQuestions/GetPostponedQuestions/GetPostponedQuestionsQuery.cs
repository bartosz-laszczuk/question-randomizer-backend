namespace QuestionRandomizer.Application.Queries.PostponedQuestions.GetPostponedQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get all postponed questions for a randomization session
/// </summary>
public record GetPostponedQuestionsQuery : IRequest<List<PostponedQuestionDto>>
{
    public string RandomizationId { get; init; } = string.Empty;
}
