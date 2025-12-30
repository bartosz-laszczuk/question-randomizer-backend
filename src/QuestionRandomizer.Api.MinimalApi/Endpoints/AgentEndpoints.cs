namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using System.Runtime.CompilerServices;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using System.Security.Claims;

/// <summary>
/// Minimal API endpoints for AI Agent operations with real-time streaming
/// </summary>
public static class AgentEndpoints
{
    public static RouteGroupBuilder MapAgentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/agent")
            .WithTags("Agent")
            .RequireAuthorization(AuthorizationPolicies.UserPolicy);

        group.MapPost("/execute", ExecuteTask)
            .WithName("ExecuteAgentTask")
            .Produces<IAsyncEnumerable<AgentStreamEvent>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async IAsyncEnumerable<AgentStreamEvent> ExecuteTask(
        ExecuteTaskRequest request,
        ClaimsPrincipal user,
        IAgentService agentService,
        ILogger<AgentStreamEvent> logger,
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

        var userId = user.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        logger.LogInformation(
            "Starting streaming execution for user {UserId} (ConversationId: {ConversationId})",
            userId, request.ConversationId ?? "none");

        await foreach (var streamEvent in agentService.ExecuteTaskStreamingAsync(
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
public record ExecuteTaskRequest(string Task, string? ConversationId = null);
