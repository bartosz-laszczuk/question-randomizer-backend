namespace QuestionRandomizer.Modules.Randomization.Application.Queries.SelectedCategories.GetSelectedCategories;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for GetSelectedCategoriesQuery
/// </summary>
public class GetSelectedCategoriesQueryHandler : IRequestHandler<GetSelectedCategoriesQuery, List<SelectedCategoryDto>>
{
    private readonly ISelectedCategoryRepository _selectedCategoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetSelectedCategoriesQueryHandler(
        ISelectedCategoryRepository selectedCategoryRepository,
        ICurrentUserService currentUserService)
    {
        _selectedCategoryRepository = selectedCategoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<SelectedCategoryDto>> Handle(GetSelectedCategoriesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var selectedCategories = await _selectedCategoryRepository.GetByRandomizationIdAsync(
            request.RandomizationId,
            userId,
            cancellationToken);

        return selectedCategories
            .Select(c => new SelectedCategoryDto
            {
                Id = c.Id,
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                CreatedAt = c.CreatedAt
            })
            .ToList();
    }
}
