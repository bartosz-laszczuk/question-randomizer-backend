namespace QuestionRandomizer.Application.Commands.PostponedQuestions.DeletePostponedQuestion;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for DeletePostponedQuestionCommand
/// </summary>
public class DeletePostponedQuestionCommandHandler : IRequestHandler<DeletePostponedQuestionCommand, Unit>
{
    private readonly IPostponedQuestionRepository _postponedQuestionRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeletePostponedQuestionCommandHandler(
        IPostponedQuestionRepository postponedQuestionRepository,
        ICurrentUserService currentUserService)
    {
        _postponedQuestionRepository = postponedQuestionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeletePostponedQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        await _postponedQuestionRepository.DeleteByQuestionIdAsync(
            request.RandomizationId,
            userId,
            request.QuestionId,
            cancellationToken);

        return Unit.Value;
    }
}
