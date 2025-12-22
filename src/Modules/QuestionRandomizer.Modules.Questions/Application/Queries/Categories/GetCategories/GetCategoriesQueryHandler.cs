namespace QuestionRandomizer.Modules.Questions.Application.Queries.Categories.GetCategories;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Handler for GetCategoriesQuery
/// </summary>
public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCategoriesQueryHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var categories = await _categoryRepository.GetByUserIdAsync(userId, cancellationToken);

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }
}
