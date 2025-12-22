namespace QuestionRandomizer.Modules.Randomization.Application.Commands.Randomizations.CreateRandomization;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Randomization.Domain.Entities;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for CreateRandomizationCommand
/// </summary>
public class CreateRandomizationCommandHandler : IRequestHandler<CreateRandomizationCommand, RandomizationDto>
{
    private readonly IRandomizationRepository _randomizationRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateRandomizationCommandHandler(
        IRandomizationRepository randomizationRepository,
        ICurrentUserService currentUserService)
    {
        _randomizationRepository = randomizationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<RandomizationDto> Handle(CreateRandomizationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var randomization = new Randomization
        {
            UserId = userId,
            ShowAnswer = false,
            Status = "Ongoing",
            CurrentQuestionId = null,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _randomizationRepository.CreateAsync(randomization, cancellationToken);

        return new RandomizationDto
        {
            Id = created.Id,
            UserId = created.UserId,
            ShowAnswer = created.ShowAnswer,
            Status = created.Status,
            CurrentQuestionId = created.CurrentQuestionId,
            CreatedAt = created.CreatedAt
        };
    }
}
