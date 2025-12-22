namespace QuestionRandomizer.Modules.Questions.Application.Commands.Qualifications.CreateQualificationsBatch;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Entities;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Handler for CreateQualificationsBatchCommand
/// </summary>
public class CreateQualificationsBatchCommandHandler : IRequestHandler<CreateQualificationsBatchCommand, List<QualificationDto>>
{
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateQualificationsBatchCommandHandler(
        IQualificationRepository qualificationRepository,
        ICurrentUserService currentUserService)
    {
        _qualificationRepository = qualificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<QualificationDto>> Handle(CreateQualificationsBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var qualifications = request.QualificationNames.Select(name => new Qualification
        {
            Name = name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        var createdQualifications = await _qualificationRepository.CreateManyAsync(qualifications, cancellationToken);

        return createdQualifications.Select(q => new QualificationDto
        {
            Id = q.Id,
            Name = q.Name,
            IsActive = q.IsActive,
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt
        }).ToList();
    }
}
