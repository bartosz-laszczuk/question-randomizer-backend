namespace QuestionRandomizer.Modules.Questions.Application.Commands.Qualifications.CreateQualificationsBatch;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;

/// <summary>
/// Command to create multiple qualifications at once
/// </summary>
public record CreateQualificationsBatchCommand : IRequest<List<QualificationDto>>
{
    public List<string> QualificationNames { get; init; } = new();
}
