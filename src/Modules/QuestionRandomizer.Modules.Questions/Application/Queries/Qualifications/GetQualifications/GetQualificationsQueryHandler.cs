namespace QuestionRandomizer.Modules.Questions.Application.Queries.Qualifications.GetQualifications;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Handler for GetQualificationsQuery
/// </summary>
public class GetQualificationsQueryHandler : IRequestHandler<GetQualificationsQuery, List<QualificationDto>>
{
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetQualificationsQueryHandler(
        IQualificationRepository qualificationRepository,
        ICurrentUserService currentUserService)
    {
        _qualificationRepository = qualificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<QualificationDto>> Handle(GetQualificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var qualifications = await _qualificationRepository.GetByUserIdAsync(userId, cancellationToken);

        return qualifications.Select(q => new QualificationDto
        {
            Id = q.Id,
            Name = q.Name,
            IsActive = q.IsActive,
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt
        }).ToList();
    }
}
