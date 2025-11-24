namespace QuestionRandomizer.Application.Commands.Qualifications.UpdateQualification;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for UpdateQualificationCommand
/// </summary>
public class UpdateQualificationCommandHandler : IRequestHandler<UpdateQualificationCommand, QualificationDto>
{
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateQualificationCommandHandler(
        IQualificationRepository qualificationRepository,
        ICurrentUserService currentUserService)
    {
        _qualificationRepository = qualificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<QualificationDto> Handle(UpdateQualificationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var qualification = await _qualificationRepository.GetByIdAsync(request.Id, userId, cancellationToken);
        if (qualification == null)
        {
            throw new NotFoundException($"Qualification with ID {request.Id} not found");
        }

        qualification.Name = request.Name;
        qualification.UpdatedAt = DateTime.UtcNow;

        var updated = await _qualificationRepository.UpdateAsync(qualification, cancellationToken);
        if (!updated)
        {
            throw new NotFoundException($"Qualification with ID {request.Id} not found");
        }

        return new QualificationDto
        {
            Id = qualification.Id,
            Name = qualification.Name,
            IsActive = qualification.IsActive,
            CreatedAt = qualification.CreatedAt,
            UpdatedAt = qualification.UpdatedAt
        };
    }
}
