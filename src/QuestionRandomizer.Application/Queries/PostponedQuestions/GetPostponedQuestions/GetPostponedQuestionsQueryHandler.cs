namespace QuestionRandomizer.Application.Queries.PostponedQuestions.GetPostponedQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for GetPostponedQuestionsQuery
/// </summary>
public class GetPostponedQuestionsQueryHandler : IRequestHandler<GetPostponedQuestionsQuery, List<PostponedQuestionDto>>
{
    private readonly IPostponedQuestionRepository _postponedQuestionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPostponedQuestionsQueryHandler(
        IPostponedQuestionRepository postponedQuestionRepository,
        ICurrentUserService currentUserService)
    {
        _postponedQuestionRepository = postponedQuestionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<PostponedQuestionDto>> Handle(GetPostponedQuestionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var postponedQuestions = await _postponedQuestionRepository.GetByRandomizationIdAsync(
            request.RandomizationId,
            userId,
            cancellationToken);

        return postponedQuestions.Select(pq => new PostponedQuestionDto
        {
            Id = pq.Id,
            QuestionId = pq.QuestionId,
            Timestamp = pq.Timestamp
        }).ToList();
    }
}
