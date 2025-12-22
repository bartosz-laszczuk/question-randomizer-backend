namespace QuestionRandomizer.Modules.Randomization.Application.Commands.UsedQuestions.UpdateUsedQuestionCategory;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for UpdateUsedQuestionCategoryCommand
/// </summary>
public class UpdateUsedQuestionCategoryCommandHandler : IRequestHandler<UpdateUsedQuestionCategoryCommand, Unit>
{
    private readonly IUsedQuestionRepository _usedQuestionRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUsedQuestionCategoryCommandHandler(
        IUsedQuestionRepository usedQuestionRepository,
        ICurrentUserService currentUserService)
    {
        _usedQuestionRepository = usedQuestionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateUsedQuestionCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        if (string.IsNullOrEmpty(request.CategoryId) || string.IsNullOrEmpty(request.CategoryName))
        {
            throw new ArgumentException("CategoryId and CategoryName are required");
        }

        await _usedQuestionRepository.UpdateCategoryAsync(
            request.RandomizationId,
            userId,
            request.CategoryId,
            request.CategoryName,
            cancellationToken);

        return Unit.Value;
    }
}
