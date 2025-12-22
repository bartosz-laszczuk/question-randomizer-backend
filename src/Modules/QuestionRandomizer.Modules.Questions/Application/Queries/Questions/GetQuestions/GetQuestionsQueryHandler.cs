namespace QuestionRandomizer.Modules.Questions.Application.Queries.Questions.GetQuestions;

using MediatR;
using QuestionRandomizer.SharedKernel.Application.Common;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Handler for GetQuestionsQuery
/// </summary>
public class GetQuestionsQueryHandler : AuthorizedHandlerBase, IRequestHandler<GetQuestionsQuery, List<QuestionDto>>
{
    private readonly IQuestionRepository _questionRepository;

    public GetQuestionsQueryHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
        : base(currentUserService)
    {
        _questionRepository = questionRepository;
    }

    public async Task<List<QuestionDto>> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

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
