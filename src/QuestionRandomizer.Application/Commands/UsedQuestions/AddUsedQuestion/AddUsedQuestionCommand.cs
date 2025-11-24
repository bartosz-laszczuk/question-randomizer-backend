namespace QuestionRandomizer.Application.Commands.UsedQuestions.AddUsedQuestion;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to add a question to the used questions list
/// </summary>
public record AddUsedQuestionCommand : IRequest<UsedQuestionDto>
{
    public string RandomizationId { get; init; } = string.Empty;
    public string QuestionId { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? CategoryName { get; init; }
}
