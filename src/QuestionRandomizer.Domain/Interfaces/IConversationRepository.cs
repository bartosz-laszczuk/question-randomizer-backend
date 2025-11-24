namespace QuestionRandomizer.Domain.Interfaces;

using QuestionRandomizer.Domain.Entities;

/// <summary>
/// Repository contract for Conversation data access
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Get all conversations for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of conversations</returns>
    Task<List<Conversation>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a conversation by its ID
    /// </summary>
    /// <param name="id">Conversation ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation if found and authorized, null otherwise</returns>
    Task<Conversation?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new conversation
    /// </summary>
    /// <param name="conversation">Conversation to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created conversation with ID</returns>
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing conversation
    /// </summary>
    /// <param name="conversation">Conversation with updated data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the updatedAt timestamp of a conversation
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated, false if not found or unauthorized</returns>
    Task<bool> UpdateTimestampAsync(string conversationId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a conversation
    /// </summary>
    /// <param name="id">Conversation ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
}
