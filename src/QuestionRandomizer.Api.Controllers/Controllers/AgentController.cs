namespace QuestionRandomizer.Api.Controllers.Controllers;

using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Controller for AI Agent task execution with real-time streaming
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.UserPolicy)]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly ILogger<AgentController> _logger;

    public AgentController(IAgentService agentService, ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    /// <summary>
    /// Execute an agent task with real-time streaming (ChatGPT-like behavior)
    /// Returns Server-Sent Events stream with task progress
    /// </summary>
    /// <param name="request">Task request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream of agent execution events</returns>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AgentStreamEvent>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async IAsyncEnumerable<AgentStreamEvent> ExecuteTask(
        [FromBody] ExecuteTaskRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Task))
        {
            yield return new AgentStreamEvent
            {
                Type = "error",
                Message = "Task description is required"
            };
            yield break;
        }

        var userId = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation(
            "Starting streaming execution for user {UserId} (ConversationId: {ConversationId})",
            userId, request.ConversationId ?? "none");

        await foreach (var streamEvent in _agentService.ExecuteTaskStreamingAsync(
            request.Task,
            userId,
            request.ConversationId,
            cancellationToken))
        {
            yield return streamEvent;
        }
    }
}

/// <summary>
/// Request model for executing a task
/// </summary>
public record ExecuteTaskRequest
{
    public string Task { get; init; } = string.Empty;
    public string? ConversationId { get; init; }
}
