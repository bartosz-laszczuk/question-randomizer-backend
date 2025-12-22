namespace QuestionRandomizer.Modules.Randomization.Application.Commands.Randomizations.UpdateRandomization;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Randomization.Domain.Entities;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for UpdateRandomizationCommand
/// </summary>
public class UpdateRandomizationCommandHandler : IRequestHandler<UpdateRandomizationCommand, RandomizationDto>
{
    private readonly IRandomizationRepository _randomizationRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRandomizationCommandHandler(
        IRandomizationRepository randomizationRepository,
        ICurrentUserService currentUserService)
    {
        _randomizationRepository = randomizationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<RandomizationDto> Handle(UpdateRandomizationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var randomization = await _randomizationRepository.GetByIdAsync(request.Id, userId, cancellationToken);
        if (randomization == null)
        {
            throw new InvalidOperationException($"Randomization with ID {request.Id} not found");
        }

        randomization.ShowAnswer = request.ShowAnswer;
        randomization.Status = request.Status;
        randomization.CurrentQuestionId = request.CurrentQuestionId;
        randomization.UpdatedAt = DateTime.UtcNow;

        await _randomizationRepository.UpdateAsync(randomization, cancellationToken);

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
