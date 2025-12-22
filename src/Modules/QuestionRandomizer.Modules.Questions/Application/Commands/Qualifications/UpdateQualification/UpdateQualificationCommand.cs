namespace QuestionRandomizer.Modules.Questions.Application.Commands.Qualifications.UpdateQualification;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Command to update an existing qualification
/// </summary>
public record UpdateQualificationCommand : IRequest<QualificationDto>
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
