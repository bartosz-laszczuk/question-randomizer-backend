namespace QuestionRandomizer.Domain.Interfaces;

using QuestionRandomizer.Domain.Entities;

/// <summary>
/// Repository contract for Randomization data access
/// </summary>
public interface IRandomizationRepository
{
    /// <summary>
    /// Get all randomization sessions for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of randomization sessions</returns>
    Task<List<Randomization>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the active randomization session for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active randomization session if found, null otherwise</returns>
    Task<Randomization?> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a randomization session by its ID
    /// </summary>
    /// <param name="id">Randomization ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Randomization session if found and authorized, null otherwise</returns>
    Task<Randomization?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new randomization session
    /// </summary>
    /// <param name="randomization">Randomization session to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created randomization session with ID</returns>
    Task<Randomization> CreateAsync(Randomization randomization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing randomization session
    /// </summary>
    /// <param name="randomization">Randomization session with updated data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateAsync(Randomization randomization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a randomization session
    /// </summary>
    /// <param name="id">Randomization ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear the current question from a randomization session
    /// </summary>
    /// <param name="randomizationId">Randomization ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if cleared, false if not found or unauthorized</returns>
    Task<bool> ClearCurrentQuestionAsync(string randomizationId, string userId, CancellationToken cancellationToken = default);
}
