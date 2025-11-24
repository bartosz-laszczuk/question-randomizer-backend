namespace QuestionRandomizer.Application.Commands.UsedQuestions.DeleteUsedQuestion;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for DeleteUsedQuestionCommand
/// </summary>
public class DeleteUsedQuestionCommandHandler : IRequestHandler<DeleteUsedQuestionCommand, Unit>
{
    private readonly IUsedQuestionRepository _usedQuestionRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUsedQuestionCommandHandler(
        IUsedQuestionRepository usedQuestionRepository,
        ICurrentUserService currentUserService)
    {
        _usedQuestionRepository = usedQuestionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteUsedQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        await _usedQuestionRepository.DeleteByQuestionIdAsync(
            request.RandomizationId,
            userId,
            request.QuestionId,
            cancellationToken);

        return Unit.Value;
    }
}
