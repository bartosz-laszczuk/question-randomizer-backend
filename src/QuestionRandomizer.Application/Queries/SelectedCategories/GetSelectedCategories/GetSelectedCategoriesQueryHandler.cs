namespace QuestionRandomizer.Application.Queries.SelectedCategories.GetSelectedCategories;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

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

        return selectedCategories.Select(sc => new SelectedCategoryDto
        {
            Id = sc.Id,
            CategoryId = sc.CategoryId,
            CategoryName = sc.CategoryName,
            CreatedAt = sc.CreatedAt
        }).ToList();
    }
}
