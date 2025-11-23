namespace QuestionRandomizer.Application.Commands.Questions.DeleteQuestion;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;
using QuestionRandomizer.Domain.Exceptions;

/// <summary>
/// Handler for DeleteQuestionCommand
/// </summary>
public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, Unit>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteQuestionCommandHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var deleted = await _questionRepository.DeleteAsync(request.Id, userId, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException("Question", request.Id);
        }

        return Unit.Value;
    }
}
