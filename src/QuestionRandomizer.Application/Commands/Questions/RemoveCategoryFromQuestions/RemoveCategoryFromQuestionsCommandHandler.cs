namespace QuestionRandomizer.Application.Commands.Questions.RemoveCategoryFromQuestions;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for RemoveCategoryFromQuestionsCommand
/// </summary>
public class RemoveCategoryFromQuestionsCommandHandler : IRequestHandler<RemoveCategoryFromQuestionsCommand, Unit>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public RemoveCategoryFromQuestionsCommandHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(RemoveCategoryFromQuestionsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        await _questionRepository.RemoveCategoryIdAsync(request.CategoryId, userId, cancellationToken);

        return Unit.Value;
    }
}
