namespace QuestionRandomizer.Modules.Questions.Application.Queries.Qualifications.GetQualificationById;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Query to get a qualification by ID
/// </summary>
public record GetQualificationByIdQuery : IRequest<QualificationDto>
{
    public string Id { get; init; } = string.Empty;
}
