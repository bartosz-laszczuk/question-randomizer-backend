namespace QuestionRandomizer.Modules.Questions.Application.Commands.Questions.UpdateQuestionsBatch;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Command to update multiple questions at once
/// </summary>
public record UpdateQuestionsBatchCommand : IRequest<Unit>
{
    public List<UpdateQuestionRequest> Questions { get; init; } = new();
}

public record UpdateQuestionRequest
{
    public string Id { get; init; } = string.Empty;
    public string QuestionText { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public string AnswerPl { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? QualificationId { get; init; }
    public List<string>? Tags { get; init; }
}
