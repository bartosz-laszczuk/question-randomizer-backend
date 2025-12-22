namespace QuestionRandomizer.Modules.Randomization.Application.Commands.UsedQuestions.DeleteUsedQuestion;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for DeleteUsedQuestionCommand
/// </summary>
public class DeleteUsedQuestionCommandHandler : IRequestHandler<DeleteUsedQuestionCommand, bool>
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

    public async Task<bool> Handle(DeleteUsedQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        return await _usedQuestionRepository.DeleteByQuestionIdAsync(
            request.RandomizationId,
            userId,
            request.QuestionId,
            cancellationToken);
    }
}
