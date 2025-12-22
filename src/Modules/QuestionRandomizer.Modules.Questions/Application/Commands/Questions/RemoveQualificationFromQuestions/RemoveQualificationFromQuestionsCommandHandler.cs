namespace QuestionRandomizer.Modules.Questions.Application.Commands.Questions.RemoveQualificationFromQuestions;

using MediatR;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Handler for RemoveQualificationFromQuestionsCommand
/// </summary>
public class RemoveQualificationFromQuestionsCommandHandler : IRequestHandler<RemoveQualificationFromQuestionsCommand, Unit>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public RemoveQualificationFromQuestionsCommandHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(RemoveQualificationFromQuestionsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        await _questionRepository.RemoveQualificationIdAsync(request.QualificationId, userId, cancellationToken);

        return Unit.Value;
    }
}
