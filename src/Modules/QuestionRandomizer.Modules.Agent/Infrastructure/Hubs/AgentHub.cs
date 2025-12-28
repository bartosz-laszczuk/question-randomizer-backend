namespace QuestionRandomizer.Modules.Agent.Infrastructure.Hubs;

using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;

/// <summary>
/// SignalR Hub for real-time AI Agent task streaming
/// Provides server-to-client streaming for queued background tasks
/// </summary>
[Authorize(Policy = AuthorizationPolicies.UserPolicy)]
public class AgentHub : Hub
{
    private readonly IAgentService _agentService;
    private readonly ILogger<AgentHub> _logger;

    public AgentHub(
        IAgentService agentService,
        ILogger<AgentHub> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    /// <summary>
    /// Stream real-time updates for a queued agent task
    /// Server-to-client streaming only - client receives updates as they occur
    /// </summary>
    /// <param name="taskId">The task ID to stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async stream of agent task events</returns>
    public async IAsyncEnumerable<AgentStreamEvent> StreamTaskUpdates(
        string taskId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var userId = Context.User?.Identity?.Name
            ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation(
            "SignalR: Starting stream for task {TaskId}, user {UserId}, connection {ConnectionId}",
            taskId, userId, Context.ConnectionId);

        // Stream updates from the agent service
        await foreach (var update in _agentService.StreamTaskUpdatesAsync(
            taskId, userId, cancellationToken))
        {
            _logger.LogDebug(
                "SignalR: Sending event {EventType} for task {TaskId} to connection {ConnectionId}",
                update.Type, taskId, Context.ConnectionId);

            yield return update;

            // Break on terminal events
            if (update.Type == "completed" || update.Type == "error")
            {
                break;
            }
        }

        _logger.LogInformation(
            "SignalR: Stream completed for task {TaskId}, connection {ConnectionId}",
            taskId, Context.ConnectionId);
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? "anonymous";
        _logger.LogInformation(
            "SignalR: Client connected - User: {UserId}, ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name ?? "anonymous";

        if (exception != null)
        {
            _logger.LogWarning(exception,
                "SignalR: Client disconnected with error - User: {UserId}, ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation(
                "SignalR: Client disconnected - User: {UserId}, ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }
}
