namespace QuestionRandomizer.Modules.Randomization.Application.Commands.UsedQuestions.AddUsedQuestion;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Randomization.Domain.Entities;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for AddUsedQuestionCommand
/// </summary>
public class AddUsedQuestionCommandHandler : IRequestHandler<AddUsedQuestionCommand, UsedQuestionDto>
{
    private readonly IUsedQuestionRepository _usedQuestionRepository;
    private readonly ICurrentUserService _currentUserService;

    public AddUsedQuestionCommandHandler(
        IUsedQuestionRepository usedQuestionRepository,
        ICurrentUserService currentUserService)
    {
        _usedQuestionRepository = usedQuestionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<UsedQuestionDto> Handle(AddUsedQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var usedQuestion = new UsedQuestion
        {
            QuestionId = request.QuestionId,
            CategoryId = request.CategoryId,
            CategoryName = request.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _usedQuestionRepository.AddAsync(
            request.RandomizationId,
            userId,
            usedQuestion,
            cancellationToken);

        return new UsedQuestionDto
        {
            Id = created.Id,
            QuestionId = created.QuestionId,
            CategoryId = created.CategoryId,
            CategoryName = created.CategoryName,
            CreatedAt = created.CreatedAt
        };
    }
}
