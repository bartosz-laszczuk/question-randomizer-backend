namespace QuestionRandomizer.Domain.Interfaces;

using QuestionRandomizer.Domain.Entities;

/// <summary>
/// Repository contract for Qualification data access
/// </summary>
public interface IQualificationRepository
{
    /// <summary>
    /// Get all qualifications for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of qualifications</returns>
    Task<List<Qualification>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a qualification by its ID
    /// </summary>
    /// <param name="id">Qualification ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Qualification if found and authorized, null otherwise</returns>
    Task<Qualification?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new qualification
    /// </summary>
    /// <param name="qualification">Qualification to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created qualification with ID</returns>
    Task<Qualification> CreateAsync(Qualification qualification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create multiple qualifications at once
    /// </summary>
    /// <param name="qualifications">Qualifications to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created qualifications with IDs</returns>
    Task<List<Qualification>> CreateManyAsync(List<Qualification> qualifications, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing qualification
    /// </summary>
    /// <param name="qualification">Qualification with updated data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateAsync(Qualification qualification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a qualification (soft delete by setting IsActive = false)
    /// </summary>
    /// <param name="id">Qualification ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
