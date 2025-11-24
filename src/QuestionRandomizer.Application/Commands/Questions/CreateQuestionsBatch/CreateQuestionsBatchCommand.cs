namespace QuestionRandomizer.Application.Commands.Questions.CreateQuestionsBatch;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create multiple questions at once
/// </summary>
public record CreateQuestionsBatchCommand : IRequest<List<QuestionDto>>
{
    public List<CreateQuestionRequest> Questions { get; init; } = new();
}

public record CreateQuestionRequest
{
    public string QuestionText { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public string AnswerPl { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? QualificationId { get; init; }
    public List<string>? Tags { get; init; }
}
