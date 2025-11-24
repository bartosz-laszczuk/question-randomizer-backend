namespace QuestionRandomizer.Application.Commands.Qualifications.DeleteQualification;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for DeleteQualificationCommand
/// </summary>
public class DeleteQualificationCommandHandler : IRequestHandler<DeleteQualificationCommand, Unit>
{
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteQualificationCommandHandler(
        IQualificationRepository qualificationRepository,
        ICurrentUserService currentUserService)
    {
        _qualificationRepository = qualificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteQualificationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var deleted = await _qualificationRepository.DeleteAsync(request.Id, userId, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Qualification with ID {request.Id} not found");
        }

        return Unit.Value;
    }
}
