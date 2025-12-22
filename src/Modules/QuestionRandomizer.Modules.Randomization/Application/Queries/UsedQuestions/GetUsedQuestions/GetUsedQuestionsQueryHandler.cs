namespace QuestionRandomizer.Modules.Randomization.Application.Queries.UsedQuestions.GetUsedQuestions;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

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

        return usedQuestions
            .Select(q => new UsedQuestionDto
            {
                Id = q.Id,
                QuestionId = q.QuestionId,
                CategoryId = q.CategoryId,
                CategoryName = q.CategoryName,
                CreatedAt = q.CreatedAt
            })
            .ToList();
    }
}
