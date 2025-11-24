namespace QuestionRandomizer.Application.Commands.Questions.UpdateQuestionsBatch;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for UpdateQuestionsBatchCommand
/// </summary>
public class UpdateQuestionsBatchCommandHandler : IRequestHandler<UpdateQuestionsBatchCommand, Unit>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateQuestionsBatchCommandHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateQuestionsBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var questions = request.Questions.Select(q => new Question
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            Answer = q.Answer,
            AnswerPl = q.AnswerPl,
            CategoryId = q.CategoryId,
            QualificationId = q.QualificationId,
            Tags = q.Tags,
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await _questionRepository.UpdateManyAsync(questions, cancellationToken);

        return Unit.Value;
    }
}
