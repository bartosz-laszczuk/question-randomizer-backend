namespace QuestionRandomizer.Application.Commands.Qualifications.CreateQualification;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create a new qualification
/// </summary>
public record CreateQualificationCommand : IRequest<QualificationDto>
{
    public string Name { get; init; } = string.Empty;
}
