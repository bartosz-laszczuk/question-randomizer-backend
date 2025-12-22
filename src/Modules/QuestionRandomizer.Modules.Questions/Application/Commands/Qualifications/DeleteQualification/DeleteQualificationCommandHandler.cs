namespace QuestionRandomizer.Modules.Questions.Application.Commands.Qualifications.DeleteQualification;

using MediatR;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.SharedKernel.Domain.Exceptions;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Events;

/// <summary>
/// Handler for DeleteQualificationCommand
/// </summary>
public class DeleteQualificationCommandHandler : IRequestHandler<DeleteQualificationCommand, Unit>
{
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public DeleteQualificationCommandHandler(
        IQualificationRepository qualificationRepository,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _qualificationRepository = qualificationRepository;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(DeleteQualificationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var deleted = await _qualificationRepository.DeleteAsync(request.Id, userId, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Qualification with ID {request.Id} not found");
        }

        // Publish domain event to clean up qualification references in questions
        await _mediator.Publish(new QualificationDeletedEvent(request.Id, userId), cancellationToken);

        return Unit.Value;
    }
}
