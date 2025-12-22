namespace QuestionRandomizer.Modules.Questions.Application.Commands.Questions.DeleteQuestion;

using MediatR;

/// <summary>
/// Command to delete a question (soft delete)
/// </summary>
public record DeleteQuestionCommand : IRequest<Unit>
{
    public string Id { get; init; } = string.Empty;
}
