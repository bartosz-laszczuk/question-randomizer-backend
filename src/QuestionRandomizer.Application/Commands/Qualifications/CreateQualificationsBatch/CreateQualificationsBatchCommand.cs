namespace QuestionRandomizer.Application.Commands.Qualifications.CreateQualificationsBatch;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create multiple qualifications at once
/// </summary>
public record CreateQualificationsBatchCommand : IRequest<List<QualificationDto>>
{
    public List<string> QualificationNames { get; init; } = new();
}
