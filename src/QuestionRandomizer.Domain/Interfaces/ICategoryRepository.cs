namespace QuestionRandomizer.Domain.Interfaces;

using QuestionRandomizer.Domain.Entities;

/// <summary>
/// Repository contract for Category data access
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Get all categories for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of categories</returns>
    Task<List<Category>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a category by its ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category if found and authorized, null otherwise</returns>
    Task<Category?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="category">Category to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created category with ID</returns>
    Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="category">Category with updated data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a category (soft delete by setting IsActive = false)
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
