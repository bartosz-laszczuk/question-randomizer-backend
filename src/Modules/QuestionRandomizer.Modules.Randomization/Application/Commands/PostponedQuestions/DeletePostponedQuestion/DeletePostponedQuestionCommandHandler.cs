namespace QuestionRandomizer.Modules.Randomization.Application.Commands.PostponedQuestions.DeletePostponedQuestion;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for DeletePostponedQuestionCommand
/// </summary>
public class DeletePostponedQuestionCommandHandler : IRequestHandler<DeletePostponedQuestionCommand, bool>
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

    public async Task<bool> Handle(DeletePostponedQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        return await _postponedQuestionRepository.DeleteByQuestionIdAsync(
            request.RandomizationId,
            userId,
            request.QuestionId,
            cancellationToken);
    }
}
