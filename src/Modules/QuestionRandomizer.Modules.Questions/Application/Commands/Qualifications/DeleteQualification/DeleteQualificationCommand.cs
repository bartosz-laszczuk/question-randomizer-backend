namespace QuestionRandomizer.Modules.Questions.Application.Commands.Qualifications.DeleteQualification;

using MediatR;

/// <summary>
/// Command to delete a qualification
/// </summary>
public record DeleteQualificationCommand : IRequest<Unit>
{
    public string Id { get; init; } = string.Empty;
}
