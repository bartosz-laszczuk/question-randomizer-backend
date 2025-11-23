namespace QuestionRandomizer.Application.Commands.Questions.CreateQuestion;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create a new question
/// </summary>
public record CreateQuestionCommand : IRequest<QuestionDto>
{
    public string QuestionText { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public string AnswerPl { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? QualificationId { get; init; }
    public List<string>? Tags { get; init; }
}
