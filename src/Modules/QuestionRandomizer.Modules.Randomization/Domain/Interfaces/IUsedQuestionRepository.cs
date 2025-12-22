namespace QuestionRandomizer.Modules.Randomization.Domain.Interfaces;

using QuestionRandomizer.Modules.Randomization.Domain.Entities;

/// <summary>
/// Repository contract for UsedQuestion data access (subcollection of Randomization)
/// </summary>
public interface IUsedQuestionRepository
{
    /// <summary>
    /// Get all used questions for a randomization session
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of used questions</returns>
    Task<List<UsedQuestion>> GetByRandomizationIdAsync(string randomizationId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a question to the used questions list
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="usedQuestion">Question to mark as used</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created used question entry</returns>
    Task<UsedQuestion> AddAsync(string randomizationId, string userId, UsedQuestion usedQuestion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a question from the used questions list
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="questionId">Question ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteByQuestionIdAsync(string randomizationId, string userId, string questionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update category information for all used questions with a specific category
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="categoryId">Category ID to update</param>
    /// <param name="categoryName">New category name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateCategoryAsync(string randomizationId, string userId, string categoryId, string categoryName, CancellationToken cancellationToken = default);
}
