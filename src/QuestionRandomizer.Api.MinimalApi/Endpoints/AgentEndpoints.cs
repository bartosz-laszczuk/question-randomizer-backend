namespace QuestionRandomizer.Api.MinimalApi.Endpoints;

using Microsoft.AspNetCore.Http.HttpResults;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using System.Security.Claims;
using System.Text.Json;

/// <summary>
/// Minimal API endpoints for AI Agent operations
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
            .Produces<AgentTaskResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/queue", QueueTask)
            .WithName("QueueAgentTask")
            .Produces<QueueTaskResponse>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/tasks/{taskId}", GetTaskStatus)
            .WithName("GetAgentTaskStatus")
            .Produces<AgentTaskStatus>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<Results<Ok<AgentTaskResult>, BadRequest<string>, StatusCodeHttpResult>> ExecuteTask(
        ExecuteTaskRequest request,
        ClaimsPrincipal user,
        IAgentService agentService,
        ILogger<AgentTaskResult> logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Task))
        {
            return TypedResults.BadRequest("Task description is required");
        }

        var userId = user.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        logger.LogInformation(
            "Executing agent task for user {UserId} (ConversationId: {ConversationId})",
            userId, request.ConversationId ?? "none");

        var result = await agentService.ExecuteTaskAsync(
            request.Task,
            userId,
            request.ConversationId,
            cancellationToken);

        if (!result.Success)
        {
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Results<AcceptedAtRoute<QueueTaskResponse>, BadRequest<string>, StatusCodeHttpResult>> QueueTask(
        ExecuteTaskRequest request,
        ClaimsPrincipal user,
        IAgentService agentService,
        ILogger<QueueTaskResponse> logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Task))
        {
            return TypedResults.BadRequest("Task description is required");
        }

        var userId = user.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        logger.LogInformation(
            "Queueing agent task for user {UserId} (ConversationId: {ConversationId})",
            userId, request.ConversationId ?? "none");

        try
        {
            var taskId = await agentService.QueueTaskAsync(
                request.Task,
                userId,
                request.ConversationId,
                cancellationToken);

            logger.LogInformation("Agent task queued with ID {TaskId}", taskId);

            var response = new QueueTaskResponse
            {
                TaskId = taskId,
                Message = "Task queued for processing"
            };

            return TypedResults.AcceptedAtRoute(response, "GetAgentTaskStatus", new { taskId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error queueing agent task");
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<Ok<AgentTaskStatus>, NotFound, StatusCodeHttpResult>> GetTaskStatus(
        string taskId,
        ClaimsPrincipal user,
        IAgentService agentService,
        ILogger<AgentTaskStatus> logger,
        CancellationToken cancellationToken)
    {
        var userId = user.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        logger.LogInformation("Getting task status for {TaskId}, user {UserId}", taskId, userId);

        try
        {
            var status = await agentService.GetTaskStatusAsync(taskId, userId, cancellationToken);

            if (status.Status == "unknown")
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting task status for {TaskId}", taskId);
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

/// <summary>
/// Request model for executing a task
/// </summary>
public record ExecuteTaskRequest(string Task, string? ConversationId = null);

/// <summary>
/// Response model for queuing a task
/// </summary>
public record QueueTaskResponse
{
    public string TaskId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
