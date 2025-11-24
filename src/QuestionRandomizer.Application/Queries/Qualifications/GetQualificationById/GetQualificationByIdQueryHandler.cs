namespace QuestionRandomizer.Application.Queries.Qualifications.GetQualificationById;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for GetQualificationByIdQuery
/// </summary>
public class GetQualificationByIdQueryHandler : IRequestHandler<GetQualificationByIdQuery, QualificationDto>
{
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetQualificationByIdQueryHandler(
        IQualificationRepository qualificationRepository,
        ICurrentUserService currentUserService)
    {
        _qualificationRepository = qualificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<QualificationDto> Handle(GetQualificationByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var qualification = await _qualificationRepository.GetByIdAsync(request.Id, userId, cancellationToken);
        if (qualification == null)
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
