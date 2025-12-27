namespace QuestionRandomizer.Modules.Agent.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Domain;

/// <summary>
/// Firestore repository for agent task persistence
/// Collection: agent_tasks
/// </summary>
public class AgentTaskRepository : IAgentTaskRepository
{
    private readonly FirestoreDb _firestore;
    private readonly ILogger<AgentTaskRepository> _logger;
    private const string CollectionName = "agent_tasks";

    public AgentTaskRepository(FirestoreDb firestore, ILogger<AgentTaskRepository> _logger)
    {
        _firestore = firestore;
        this._logger = _logger;
    }

    public async Task<AgentTask> CreateAsync(AgentTask task, CancellationToken cancellationToken = default)
    {
        var docRef = _firestore.Collection(CollectionName).Document(task.TaskId);

        var data = new Dictionary<string, object>
        {
            { "taskId", task.TaskId },
            { "userId", task.UserId },
            { "taskDescription", task.TaskDescription },
            { "status", task.Status },
            { "createdAt", task.CreatedAt },
            { "metadata", task.Metadata ?? new Dictionary<string, object>() }
        };

        if (task.JobId != null)
            data["jobId"] = task.JobId;

        await docRef.SetAsync(data, cancellationToken: cancellationToken);

        _logger.LogInformation("Created agent task {TaskId} for user {UserId}", task.TaskId, task.UserId);

        return task;
    }

    public async Task<AgentTask?> GetByIdAsync(string taskId, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestore.Collection(CollectionName).Document(taskId);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
            return null;

        var task = ConvertToAgentTask(snapshot);

        // Security: Verify userId matches
        if (task.UserId != userId)
        {
            _logger.LogWarning(
                "Unauthorized access attempt: Task {TaskId} requested by {RequestUserId} but belongs to {OwnerUserId}",
                taskId, userId, task.UserId);
            return null;
        }

        return task;
    }

    public async Task UpdateAsync(AgentTask task, CancellationToken cancellationToken = default)
    {
        var docRef = _firestore.Collection(CollectionName).Document(task.TaskId);

        var updates = new Dictionary<string, object>
        {
            { "status", task.Status },
            { "metadata", task.Metadata ?? new Dictionary<string, object>() }
        };

        if (task.Result != null)
            updates["result"] = task.Result;

        if (task.Error != null)
            updates["error"] = task.Error;

        if (task.StartedAt.HasValue)
            updates["startedAt"] = task.StartedAt.Value;

        if (task.CompletedAt.HasValue)
            updates["completedAt"] = task.CompletedAt.Value;

        await docRef.UpdateAsync(updates, cancellationToken: cancellationToken);

        _logger.LogDebug("Updated agent task {TaskId} with status {Status}", task.TaskId, task.Status);
    }

    public async Task<List<AgentTask>> GetByUserIdAsync(
        string userId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _firestore.Collection(CollectionName)
            .WhereEqualTo("userId", userId)
            .OrderByDescending("createdAt")
            .Limit(limit);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(ConvertToAgentTask)
            .ToList();
    }

    public async Task UpdateStatusAsync(
        string taskId,
        string userId,
        string status,
        DateTime? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        var task = await GetByIdAsync(taskId, userId, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for status update", taskId);
            return;
        }

        var updates = new Dictionary<string, object>
        {
            { "status", status }
        };

        // Update appropriate timestamp based on status
        if (status == "processing" && !task.StartedAt.HasValue)
        {
            updates["startedAt"] = timestamp ?? DateTime.UtcNow;
        }
        else if (status is "completed" or "failed" && !task.CompletedAt.HasValue)
        {
            updates["completedAt"] = timestamp ?? DateTime.UtcNow;
        }

        var docRef = _firestore.Collection(CollectionName).Document(taskId);
        await docRef.UpdateAsync(updates, cancellationToken: cancellationToken);

        _logger.LogInformation("Updated task {TaskId} status to {Status}", taskId, status);
    }

    public async Task SetResultAsync(
        string taskId,
        string userId,
        string result,
        CancellationToken cancellationToken = default)
    {
        var task = await GetByIdAsync(taskId, userId, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for result update", taskId);
            return;
        }

        var updates = new Dictionary<string, object>
        {
            { "result", result },
            { "status", "completed" },
            { "completedAt", DateTime.UtcNow }
        };

        var docRef = _firestore.Collection(CollectionName).Document(taskId);
        await docRef.UpdateAsync(updates, cancellationToken: cancellationToken);

        _logger.LogInformation("Set result for task {TaskId}", taskId);
    }

    public async Task SetErrorAsync(
        string taskId,
        string userId,
        string error,
        CancellationToken cancellationToken = default)
    {
        var task = await GetByIdAsync(taskId, userId, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for error update", taskId);
            return;
        }

        var updates = new Dictionary<string, object>
        {
            { "error", error },
            { "status", "failed" },
            { "completedAt", DateTime.UtcNow }
        };

        var docRef = _firestore.Collection(CollectionName).Document(taskId);
        await docRef.UpdateAsync(updates, cancellationToken: cancellationToken);

        _logger.LogError("Set error for task {TaskId}: {Error}", taskId, error);
    }

    private static AgentTask ConvertToAgentTask(DocumentSnapshot snapshot)
    {
        var data = snapshot.ToDictionary();

        return new AgentTask
        {
            TaskId = data.GetValueOrDefault("taskId")?.ToString() ?? snapshot.Id,
            UserId = data.GetValueOrDefault("userId")?.ToString() ?? string.Empty,
            TaskDescription = data.GetValueOrDefault("taskDescription")?.ToString() ?? string.Empty,
            Status = data.GetValueOrDefault("status")?.ToString() ?? "unknown",
            Result = data.GetValueOrDefault("result")?.ToString(),
            Error = data.GetValueOrDefault("error")?.ToString(),
            JobId = data.GetValueOrDefault("jobId")?.ToString(),
            CreatedAt = data.ContainsKey("createdAt")
                ? ((Timestamp)data["createdAt"]).ToDateTime()
                : DateTime.UtcNow,
            StartedAt = data.ContainsKey("startedAt")
                ? ((Timestamp)data["startedAt"]).ToDateTime()
                : null,
            CompletedAt = data.ContainsKey("completedAt")
                ? ((Timestamp)data["completedAt"]).ToDateTime()
                : null,
            Metadata = data.GetValueOrDefault("metadata") as Dictionary<string, object>
        };
    }
}
