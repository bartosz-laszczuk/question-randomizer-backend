namespace QuestionRandomizer.Modules.Randomization.Application.Queries.PostponedQuestions.GetPostponedQuestions;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

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

        return postponedQuestions
            .Select(q => new PostponedQuestionDto
            {
                Id = q.Id,
                QuestionId = q.QuestionId,
                Timestamp = q.Timestamp
            })
            .ToList();
    }
}
