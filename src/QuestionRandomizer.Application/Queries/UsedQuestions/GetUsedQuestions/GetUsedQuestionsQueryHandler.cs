namespace QuestionRandomizer.Application.Queries.UsedQuestions.GetUsedQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for GetUsedQuestionsQuery
/// </summary>
public class GetUsedQuestionsQueryHandler : IRequestHandler<GetUsedQuestionsQuery, List<UsedQuestionDto>>
{
    private readonly IUsedQuestionRepository _usedQuestionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUsedQuestionsQueryHandler(
        IUsedQuestionRepository usedQuestionRepository,
        ICurrentUserService currentUserService)
    {
        _usedQuestionRepository = usedQuestionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<UsedQuestionDto>> Handle(GetUsedQuestionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var usedQuestions = await _usedQuestionRepository.GetByRandomizationIdAsync(
            request.RandomizationId,
            userId,
            cancellationToken);

        return usedQuestions.Select(uq => new UsedQuestionDto
        {
            Id = uq.Id,
            QuestionId = uq.QuestionId,
            CategoryId = uq.CategoryId,
            CategoryName = uq.CategoryName,
            CreatedAt = uq.CreatedAt
        }).ToList();
    }
}
