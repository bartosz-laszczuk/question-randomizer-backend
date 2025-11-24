namespace QuestionRandomizer.Application.Queries.Qualifications.GetQualifications;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get all qualifications for the current user
/// </summary>
public record GetQualificationsQuery : IRequest<List<QualificationDto>>
{
}
