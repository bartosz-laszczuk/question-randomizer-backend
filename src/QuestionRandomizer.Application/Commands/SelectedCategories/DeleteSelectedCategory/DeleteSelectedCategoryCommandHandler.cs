namespace QuestionRandomizer.Application.Commands.SelectedCategories.DeleteSelectedCategory;

using MediatR;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for DeleteSelectedCategoryCommand
/// </summary>
public class DeleteSelectedCategoryCommandHandler : IRequestHandler<DeleteSelectedCategoryCommand, Unit>
{
    private readonly ISelectedCategoryRepository _selectedCategoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSelectedCategoryCommandHandler(
        ISelectedCategoryRepository selectedCategoryRepository,
        ICurrentUserService currentUserService)
    {
        _selectedCategoryRepository = selectedCategoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteSelectedCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        await _selectedCategoryRepository.DeleteByCategoryIdAsync(
            request.RandomizationId,
            userId,
            request.CategoryId,
            cancellationToken);

        return Unit.Value;
    }
}
