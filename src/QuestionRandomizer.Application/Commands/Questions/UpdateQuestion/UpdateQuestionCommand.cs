namespace QuestionRandomizer.Application.Commands.Questions.UpdateQuestion;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to update an existing question
/// </summary>
public record UpdateQuestionCommand : IRequest<QuestionDto>
{
    public string Id { get; init; } = string.Empty;
    public string QuestionText { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public string AnswerPl { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? QualificationId { get; init; }
    public List<string>? Tags { get; init; }
}
