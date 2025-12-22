namespace QuestionRandomizer.Modules.Conversations.Domain.Interfaces;

using QuestionRandomizer.Modules.Conversations.Domain.Entities;

/// <summary>
/// Repository contract for Message data access
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Get all messages for a specific conversation
    /// </summary>
    /// <param name="conversationId">The conversation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of messages ordered by timestamp</returns>
    Task<List<Message>> GetByConversationIdAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a message by its ID
    /// </summary>
    /// <param name="id">Message ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Message if found, null otherwise</returns>
    Task<Message?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new message
    /// </summary>
    /// <param name="message">Message to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created message with ID</returns>
    Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a message
    /// </summary>
    /// <param name="id">Message ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
