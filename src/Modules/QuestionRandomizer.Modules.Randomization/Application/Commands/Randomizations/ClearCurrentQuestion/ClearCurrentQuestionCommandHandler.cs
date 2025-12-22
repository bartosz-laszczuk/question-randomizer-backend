namespace QuestionRandomizer.Modules.Randomization.Application.Commands.Randomizations.ClearCurrentQuestion;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

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

        var randomization = await _randomizationRepository.GetByIdAsync(request.RandomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            throw new InvalidOperationException($"Randomization with ID {request.RandomizationId} not found");
        }

        randomization.CurrentQuestionId = null;
        randomization.UpdatedAt = DateTime.UtcNow;

        await _randomizationRepository.UpdateAsync(randomization, cancellationToken);

        return Unit.Value;
    }
}
