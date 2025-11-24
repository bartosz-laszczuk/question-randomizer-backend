namespace QuestionRandomizer.Application.Commands.Randomizations.UpdateRandomization;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Exceptions;
using QuestionRandomizer.Domain.Interfaces;

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
            throw new NotFoundException($"Randomization with ID {request.Id} not found");
        }

        randomization.ShowAnswer = request.ShowAnswer;
        randomization.Status = request.Status;
        randomization.CurrentQuestionId = request.CurrentQuestionId;

        var updated = await _randomizationRepository.UpdateAsync(randomization, cancellationToken);
        if (!updated)
        {
            throw new NotFoundException($"Randomization with ID {request.Id} not found");
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
