namespace QuestionRandomizer.Application.Queries.Questions.GetQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for GetQuestionsQuery
/// </summary>
public class GetQuestionsQueryHandler : IRequestHandler<GetQuestionsQuery, List<QuestionDto>>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetQuestionsQueryHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<QuestionDto>> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var questions = string.IsNullOrEmpty(request.CategoryId)
            ? await _questionRepository.GetByUserIdAsync(userId, cancellationToken)
            : await _questionRepository.GetByCategoryIdAsync(request.CategoryId, userId, cancellationToken);

        if (request.IsActive.HasValue)
        {
            questions = questions.Where(q => q.IsActive == request.IsActive.Value).ToList();
        }

        return questions.Select(q => new QuestionDto
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
            Tags = q.Tags,
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt
        }).ToList();
    }
}
