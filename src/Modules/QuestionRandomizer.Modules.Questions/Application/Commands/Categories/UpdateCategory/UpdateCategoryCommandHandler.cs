namespace QuestionRandomizer.Modules.Questions.Application.Commands.Categories.UpdateCategory;

using MediatR;
using QuestionRandomizer.Modules.Questions.Application.DTOs;
using QuestionRandomizer.SharedKernel.Application.Interfaces;
using QuestionRandomizer.SharedKernel.Domain.Exceptions;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;

/// <summary>
/// Handler for UpdateCategoryCommand
/// </summary>
public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var category = await _categoryRepository.GetByIdAsync(request.Id, userId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found");
        }

        category.Name = request.Name;
        category.UpdatedAt = DateTime.UtcNow;

        var updated = await _categoryRepository.UpdateAsync(category, cancellationToken);
        if (!updated)
        {
            throw new NotFoundException($"Category with ID {request.Id} not found");
        }

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
