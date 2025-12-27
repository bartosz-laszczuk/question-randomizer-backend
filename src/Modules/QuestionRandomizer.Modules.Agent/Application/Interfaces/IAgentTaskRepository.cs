namespace QuestionRandomizer.Modules.Agent.Application.Interfaces;

using QuestionRandomizer.Modules.Agent.Domain;

/// <summary>
/// Repository interface for agent task persistence in Firestore
/// </summary>
public interface IAgentTaskRepository
{
    /// <summary>
    /// Create a new agent task
    /// </summary>
    Task<AgentTask> CreateAsync(AgentTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an agent task by ID
    /// </summary>
    Task<AgentTask?> GetByIdAsync(string taskId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing agent task
    /// </summary>
    Task UpdateAsync(AgentTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all tasks for a user
    /// </summary>
    Task<List<AgentTask>> GetByUserIdAsync(string userId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update task status
    /// </summary>
    Task UpdateStatusAsync(
        string taskId,
        string userId,
        string status,
        DateTime? timestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Set task result
    /// </summary>
    Task SetResultAsync(
        string taskId,
        string userId,
        string result,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Set task error
    /// </summary>
    Task SetErrorAsync(
        string taskId,
        string userId,
        string error,
        CancellationToken cancellationToken = default);
}
