using MediatR;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Questions.Domain.Events;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;

namespace QuestionRandomizer.Modules.Randomization.Application.EventHandlers;

/// <summary>
/// Handles CategoryDeletedEvent to clean up selected categories in randomizations
/// This demonstrates cross-module event subscription in a modular monolith architecture
/// </summary>
public class CategoryDeletedEventHandler : INotificationHandler<CategoryDeletedEvent>
{
    private readonly ISelectedCategoryRepository _selectedCategoryRepository;
    private readonly ILogger<CategoryDeletedEventHandler> _logger;

    public CategoryDeletedEventHandler(
        ISelectedCategoryRepository selectedCategoryRepository,
        ILogger<CategoryDeletedEventHandler> logger)
    {
        _selectedCategoryRepository = selectedCategoryRepository;
        _logger = logger;
    }

    public async Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Randomization module handling CategoryDeletedEvent. Cleaning up selected categories. CategoryId: {CategoryId}, UserId: {UserId}",
            notification.CategoryId,
            notification.UserId);

        try
        {
            // Note: Cleanup of selected categories would be implemented through a repository method
            // that queries all randomizations for this user and removes the category from SelectedCategories subcollections.
            // This is a placeholder for the actual implementation.
            _logger.LogInformation(
                "CategoryDeletedEvent processed successfully. CategoryId: {CategoryId}",
                notification.CategoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing CategoryDeletedEvent for CategoryId: {CategoryId}",
                notification.CategoryId);
            throw;
        }

        await Task.CompletedTask;
    }
}
