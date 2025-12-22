namespace QuestionRandomizer.Modules.Randomization.Application.Commands.SelectedCategories.DeleteSelectedCategory;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for DeleteSelectedCategoryCommand
/// </summary>
public class DeleteSelectedCategoryCommandHandler : IRequestHandler<DeleteSelectedCategoryCommand, bool>
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

    public async Task<bool> Handle(DeleteSelectedCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        return await _selectedCategoryRepository.DeleteByCategoryIdAsync(
            request.RandomizationId,
            userId,
            request.CategoryId,
            cancellationToken);
    }
}
