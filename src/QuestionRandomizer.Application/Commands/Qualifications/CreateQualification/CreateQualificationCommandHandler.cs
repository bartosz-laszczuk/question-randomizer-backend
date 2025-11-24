namespace QuestionRandomizer.Application.Commands.Qualifications.CreateQualification;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for CreateQualificationCommand
/// </summary>
public class CreateQualificationCommandHandler : IRequestHandler<CreateQualificationCommand, QualificationDto>
{
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateQualificationCommandHandler(
        IQualificationRepository qualificationRepository,
        ICurrentUserService currentUserService)
    {
        _qualificationRepository = qualificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<QualificationDto> Handle(CreateQualificationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var qualification = new Qualification
        {
            Name = request.Name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdQualification = await _qualificationRepository.CreateAsync(qualification, cancellationToken);

        return new QualificationDto
        {
            Id = createdQualification.Id,
            Name = createdQualification.Name,
            IsActive = createdQualification.IsActive,
            CreatedAt = createdQualification.CreatedAt,
            UpdatedAt = createdQualification.UpdatedAt
        };
    }
}
