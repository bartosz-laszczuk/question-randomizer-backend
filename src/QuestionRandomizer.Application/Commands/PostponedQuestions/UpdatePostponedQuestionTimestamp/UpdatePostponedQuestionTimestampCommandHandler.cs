namespace QuestionRandomizer.Application.Commands.PostponedQuestions.UpdatePostponedQuestionTimestamp;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

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

        await _postponedQuestionRepository.UpdateTimestampAsync(
            request.RandomizationId,
            userId,
            request.QuestionId,
            cancellationToken);

        return Unit.Value;
    }
}
