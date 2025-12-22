namespace QuestionRandomizer.Modules.Randomization.Domain.Interfaces;

using QuestionRandomizer.Modules.Randomization.Domain.Entities;

/// <summary>
/// Repository contract for SelectedCategory data access (subcollection of Randomization)
/// </summary>
public interface ISelectedCategoryRepository
{
    /// <summary>
    /// Get all selected categories for a randomization session
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of selected categories</returns>
    Task<List<SelectedCategory>> GetByRandomizationIdAsync(string randomizationId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a category to the selected categories for a randomization session
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="selectedCategory">Category to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created selected category</returns>
    Task<SelectedCategory> AddAsync(string randomizationId, string userId, SelectedCategory selectedCategory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a category from the selected categories
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="categoryId">Category ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteByCategoryIdAsync(string randomizationId, string userId, string categoryId, CancellationToken cancellationToken = default);
}
