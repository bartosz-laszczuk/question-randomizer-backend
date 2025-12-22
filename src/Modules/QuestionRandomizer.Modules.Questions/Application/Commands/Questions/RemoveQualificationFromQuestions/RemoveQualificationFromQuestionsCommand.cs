namespace QuestionRandomizer.Modules.Questions.Application.Commands.Questions.RemoveQualificationFromQuestions;

using MediatR;

/// <summary>
/// Command to remove a qualification ID from all questions
/// </summary>
public record RemoveQualificationFromQuestionsCommand : IRequest<Unit>
{
    public string QualificationId { get; init; } = string.Empty;
}
