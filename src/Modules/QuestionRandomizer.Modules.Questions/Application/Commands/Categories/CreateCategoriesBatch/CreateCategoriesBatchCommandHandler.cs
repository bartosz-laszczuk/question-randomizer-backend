namespace QuestionRandomizer.Modules.Questions.Application.Commands.Categories.CreateCategoriesBatch;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.Modules.Questions.Domain.Entities;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Handler for CreateCategoriesBatchCommand
/// </summary>
public class CreateCategoriesBatchCommandHandler : IRequestHandler<CreateCategoriesBatchCommand, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateCategoriesBatchCommandHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<CategoryDto>> Handle(CreateCategoriesBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var categories = request.CategoryNames.Select(name => new Category
        {
            Name = name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        var createdCategories = await _categoryRepository.CreateManyAsync(categories, cancellationToken);

        return createdCategories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }
}
