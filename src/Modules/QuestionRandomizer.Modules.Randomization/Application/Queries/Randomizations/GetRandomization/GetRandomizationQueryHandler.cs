namespace QuestionRandomizer.Modules.Randomization.Application.Queries.Randomizations.GetRandomization;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for GetRandomizationQuery
/// </summary>
public class GetRandomizationQueryHandler : IRequestHandler<GetRandomizationQuery, RandomizationDto?>
{
    private readonly IRandomizationRepository _randomizationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetRandomizationQueryHandler(
        IRandomizationRepository randomizationRepository,
        ICurrentUserService currentUserService)
    {
        _randomizationRepository = randomizationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<RandomizationDto?> Handle(GetRandomizationQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var randomization = await _randomizationRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (randomization == null)
        {
            return null;
        }

        return new RandomizationDto
        {
            Id = randomization.Id,
            UserId = randomization.UserId,
            ShowAnswer = randomization.ShowAnswer,
            Status = randomization.Status,
            CurrentQuestionId = randomization.CurrentQuestionId,
            CreatedAt = randomization.CreatedAt
        };
    }
}
