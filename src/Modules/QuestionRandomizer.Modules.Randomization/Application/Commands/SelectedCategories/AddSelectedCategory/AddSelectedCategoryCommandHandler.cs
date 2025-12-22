namespace QuestionRandomizer.Modules.Randomization.Application.Commands.SelectedCategories.AddSelectedCategory;

using MediatR;
using QuestionRandomizer.Modules.Randomization.Application.DTOs;
using QuestionRandomizer.Modules.Randomization.Domain.Entities;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Application.Interfaces;

/// <summary>
/// Handler for AddSelectedCategoryCommand
/// </summary>
public class AddSelectedCategoryCommandHandler : IRequestHandler<AddSelectedCategoryCommand, SelectedCategoryDto>
{
    private readonly ISelectedCategoryRepository _selectedCategoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public AddSelectedCategoryCommandHandler(
        ISelectedCategoryRepository selectedCategoryRepository,
        ICurrentUserService currentUserService)
    {
        _selectedCategoryRepository = selectedCategoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<SelectedCategoryDto> Handle(AddSelectedCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var selectedCategory = new SelectedCategory
        {
            CategoryId = request.CategoryId,
            CategoryName = request.CategoryName,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _selectedCategoryRepository.AddAsync(
            request.RandomizationId,
            userId,
            selectedCategory,
            cancellationToken);

        return new SelectedCategoryDto
        {
            Id = created.Id,
            CategoryId = created.CategoryId,
            CategoryName = created.CategoryName,
            CreatedAt = created.CreatedAt
        };
    }
}
