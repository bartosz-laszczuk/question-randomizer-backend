namespace QuestionRandomizer.Modules.Randomization.Domain.Interfaces;

using QuestionRandomizer.Modules.Randomization.Domain.Entities;

/// <summary>
/// Repository contract for PostponedQuestion data access (subcollection of Randomization)
/// </summary>
public interface IPostponedQuestionRepository
{
    /// <summary>
    /// Get all postponed questions for a randomization session
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of postponed questions</returns>
    Task<List<PostponedQuestion>> GetByRandomizationIdAsync(string randomizationId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a question to the postponed questions list
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="postponedQuestion">Question to postpone</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created postponed question entry</returns>
    Task<PostponedQuestion> AddAsync(string randomizationId, string userId, PostponedQuestion postponedQuestion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a question from the postponed questions list
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="questionId">Question ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteByQuestionIdAsync(string randomizationId, string userId, string questionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the timestamp for a postponed question
    /// </summary>
    /// <param name="randomizationId">Randomization session ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="questionId">Question ID to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated, false if not found or unauthorized</returns>
    Task<bool> UpdateTimestampAsync(string randomizationId, string userId, string questionId, CancellationToken cancellationToken = default);
}
