namespace QuestionRandomizer.Modules.Questions.Application.Commands.Qualifications.CreateQualification;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Command to create a new qualification
/// </summary>
public record CreateQualificationCommand : IRequest<QualificationDto>
{
    public string Name { get; init; } = string.Empty;
}
