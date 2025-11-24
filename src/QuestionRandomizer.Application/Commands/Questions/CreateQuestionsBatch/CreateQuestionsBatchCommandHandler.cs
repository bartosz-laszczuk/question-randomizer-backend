namespace QuestionRandomizer.Application.Commands.Questions.CreateQuestionsBatch;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for CreateQuestionsBatchCommand
/// </summary>
public class CreateQuestionsBatchCommandHandler : IRequestHandler<CreateQuestionsBatchCommand, List<QuestionDto>>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateQuestionsBatchCommandHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<QuestionDto>> Handle(CreateQuestionsBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var questions = request.Questions.Select(q => new Question
        {
            QuestionText = q.QuestionText,
            Answer = q.Answer,
            AnswerPl = q.AnswerPl,
            CategoryId = q.CategoryId,
            QualificationId = q.QualificationId,
            Tags = q.Tags,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        var created = await _questionRepository.CreateManyAsync(questions, cancellationToken);

        return created.Select(q => new QuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            Answer = q.Answer,
            AnswerPl = q.AnswerPl,
            CategoryId = q.CategoryId,
            CategoryName = q.CategoryName,
            QualificationId = q.QualificationId,
            QualificationName = q.QualificationName,
            IsActive = q.IsActive,
            Tags = q.Tags
        }).ToList();
    }
}
