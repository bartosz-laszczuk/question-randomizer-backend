namespace QuestionRandomizer.Modules.Questions.Application.Queries.Qualifications.GetQualifications;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Query to get all qualifications for the current user
/// </summary>
public record GetQualificationsQuery : IRequest<List<QualificationDto>>
{
}
