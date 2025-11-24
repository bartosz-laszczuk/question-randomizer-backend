namespace QuestionRandomizer.Application.Queries.UsedQuestions.GetUsedQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get all used questions for a randomization session
/// </summary>
public record GetUsedQuestionsQuery : IRequest<List<UsedQuestionDto>>
{
    public string RandomizationId { get; init; } = string.Empty;
}
