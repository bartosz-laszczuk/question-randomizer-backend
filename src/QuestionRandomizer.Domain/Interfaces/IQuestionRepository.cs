namespace QuestionRandomizer.Domain.Interfaces;

using QuestionRandomizer.Domain.Entities;

/// <summary>
/// Repository contract for Question data access
/// </summary>
public interface IQuestionRepository
{
    /// <summary>
    /// Get all questions for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of questions</returns>
    Task<List<Question>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a question by its ID
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Question if found and authorized, null otherwise</returns>
    Task<Question?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new question
    /// </summary>
    /// <param name="question">Question to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created question with ID</returns>
    Task<Question> CreateAsync(Question question, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create multiple questions at once
    /// </summary>
    /// <param name="questions">Questions to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created questions with IDs</returns>
    Task<List<Question>> CreateManyAsync(List<Question> questions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing question
    /// </summary>
    /// <param name="question">Question with updated data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateAsync(Question question, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update multiple questions at once
    /// </summary>
    /// <param name="questions">Questions to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdateManyAsync(List<Question> questions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a question (soft delete by setting IsActive = false)
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get questions by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of questions in the category</returns>
    Task<List<Question>> GetByCategoryIdAsync(string categoryId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove category ID from all questions that reference it
    /// </summary>
    /// <param name="categoryId">Category ID to remove</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task RemoveCategoryIdAsync(string categoryId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove qualification ID from all questions that reference it
    /// </summary>
    /// <param name="qualificationId">Qualification ID to remove</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task RemoveQualificationIdAsync(string qualificationId, string userId, CancellationToken cancellationToken = default);
}
