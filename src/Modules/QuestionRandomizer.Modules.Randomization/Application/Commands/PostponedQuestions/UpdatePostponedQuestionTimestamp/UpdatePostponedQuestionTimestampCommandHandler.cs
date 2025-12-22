namespace QuestionRandomizer.Modules.Randomization.Application.Commands.PostponedQuestions.UpdatePostponedQuestionTimestamp;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for UpdatePostponedQuestionTimestampCommand
/// </summary>
public class UpdatePostponedQuestionTimestampCommandHandler : IRequestHandler<UpdatePostponedQuestionTimestampCommand, Unit>
{
    private readonly IPostponedQuestionRepository _postponedQuestionRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePostponedQuestionTimestampCommandHandler(
        IPostponedQuestionRepository postponedQuestionRepository,
        ICurrentUserService currentUserService)
    {
        _postponedQuestionRepository = postponedQuestionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdatePostponedQuestionTimestampCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var updated = await _postponedQuestionRepository.UpdateTimestampAsync(
            request.RandomizationId,
            userId,
            request.QuestionId,
            cancellationToken);

        if (!updated)
        {
            throw new InvalidOperationException($"Postponed question with ID {request.QuestionId} not found or unauthorized");
        }

        return Unit.Value;
    }
}
