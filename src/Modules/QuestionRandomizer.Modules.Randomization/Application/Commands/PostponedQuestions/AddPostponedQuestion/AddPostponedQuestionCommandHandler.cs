namespace QuestionRandomizer.Modules.Randomization.Application.Commands.PostponedQuestions.AddPostponedQuestion;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Randomization.Domain.Entities;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for AddPostponedQuestionCommand
/// </summary>
public class AddPostponedQuestionCommandHandler : IRequestHandler<AddPostponedQuestionCommand, PostponedQuestionDto>
{
    private readonly IPostponedQuestionRepository _postponedQuestionRepository;
    private readonly ICurrentUserService _currentUserService;

    public AddPostponedQuestionCommandHandler(
        IPostponedQuestionRepository postponedQuestionRepository,
        ICurrentUserService currentUserService)
    {
        _postponedQuestionRepository = postponedQuestionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PostponedQuestionDto> Handle(AddPostponedQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var postponedQuestion = new PostponedQuestion
        {
            QuestionId = request.QuestionId,
            Timestamp = DateTime.UtcNow
        };

        var created = await _postponedQuestionRepository.AddAsync(
            request.RandomizationId,
            userId,
            postponedQuestion,
            cancellationToken);

        return new PostponedQuestionDto
        {
            Id = created.Id,
            QuestionId = created.QuestionId,
            Timestamp = created.Timestamp
        };
    }
}
