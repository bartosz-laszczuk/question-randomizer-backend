namespace QuestionRandomizer.Application.Commands.Randomizations.ClearCurrentQuestion;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for ClearCurrentQuestionCommand
/// </summary>
public class ClearCurrentQuestionCommandHandler : IRequestHandler<ClearCurrentQuestionCommand, Unit>
{
    private readonly IRandomizationRepository _randomizationRepository;
    private readonly ICurrentUserService _currentUserService;

    public ClearCurrentQuestionCommandHandler(
        IRandomizationRepository randomizationRepository,
        ICurrentUserService currentUserService)
    {
        _randomizationRepository = randomizationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ClearCurrentQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var cleared = await _randomizationRepository.ClearCurrentQuestionAsync(request.RandomizationId, userId, cancellationToken);
        if (!cleared)
        {
            throw new NotFoundException($"Randomization with ID {request.RandomizationId} not found");
        }

        return Unit.Value;
    }
}
