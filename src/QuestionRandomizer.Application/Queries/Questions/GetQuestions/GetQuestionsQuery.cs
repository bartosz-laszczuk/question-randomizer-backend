namespace QuestionRandomizer.Application.Queries.Questions.GetQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get all questions for the current user
/// </summary>
public record GetQuestionsQuery : IRequest<List<QuestionDto>>
{
    public string? CategoryId { get; init; }
    public bool? IsActive { get; init; }
}
