namespace QuestionRandomizer.Modules.Questions.Application.Queries.Questions.GetQuestionById;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Query to get a question by its ID
/// </summary>
public record GetQuestionByIdQuery : IRequest<QuestionDto>
{
    public string Id { get; init; } = string.Empty;
}
