namespace QuestionRandomizer.Api.Controllers.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.SharedKernel.Infrastructure.Authorization;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Controller for AI Agent task execution
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
    /// Execute an agent task synchronously
    /// </summary>
    /// <param name="request">Task request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task result</returns>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(AgentTaskResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AgentTaskResult>> ExecuteTask(
        [FromBody] ExecuteTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Task))
        {
            return BadRequest("Task description is required");
        }

        var userId = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation(
            "Executing agent task for user {UserId} (ConversationId: {ConversationId})",
            userId, request.ConversationId ?? "none");

        var result = await _agentService.ExecuteTaskAsync(
            request.Task,
            userId,
            request.ConversationId,
            cancellationToken);

        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Execute an agent task with streaming progress updates
    /// </summary>
    /// <param name="request">Task request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SSE stream of progress updates</returns>
    [HttpPost("execute/stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task ExecuteTaskStreaming(
        [FromBody] ExecuteTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Task))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsync("Task description is required", cancellationToken);
            return;
        }

        var userId = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation(
            "Executing streaming agent task for user {UserId} (ConversationId: {ConversationId})",
            userId, request.ConversationId ?? "none");

        // Set SSE headers
        Response.ContentType = "text/event-stream";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        try
        {
            await _agentService.ExecuteTaskStreamingAsync(
                request.Task,
                userId,
                streamEvent =>
                {
                    // Forward stream events to client
                    var eventData = System.Text.Json.JsonSerializer.Serialize(streamEvent);
                    Response.WriteAsync($"event: {streamEvent.Type}\n", cancellationToken).Wait();
                    Response.WriteAsync($"data: {eventData}\n\n", cancellationToken).Wait();
                    Response.Body.FlushAsync(cancellationToken).Wait();
                },
                request.ConversationId,
                cancellationToken);

            _logger.LogInformation("Streaming agent task completed for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during streaming agent task execution");
            var errorEvent = new { type = "error", message = ex.Message };
            var errorData = System.Text.Json.JsonSerializer.Serialize(errorEvent);
            await Response.WriteAsync($"event: error\n", cancellationToken);
            await Response.WriteAsync($"data: {errorData}\n\n", cancellationToken);
        }
    }

    /// <summary>
    /// Queue an agent task for background processing
    /// </summary>
    /// <param name="request">Task request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for tracking</returns>
    [HttpPost("queue")]
    [ProducesResponseType(typeof(QueueTaskResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QueueTaskResponse>> QueueTask(
        [FromBody] ExecuteTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Task))
        {
            return BadRequest("Task description is required");
        }

        var userId = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation(
            "Queueing agent task for user {UserId} (ConversationId: {ConversationId})",
            userId, request.ConversationId ?? "none");

        try
        {
            var taskId = await _agentService.QueueTaskAsync(
                request.Task,
                userId,
                request.ConversationId,
                cancellationToken);

            _logger.LogInformation("Agent task queued with ID {TaskId}", taskId);

            return Accepted(new QueueTaskResponse
            {
                TaskId = taskId,
                Message = "Task queued for processing"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing agent task");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Error = "Failed to queue task",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Get the status of a queued agent task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task status</returns>
    [HttpGet("tasks/{taskId}")]
    [ProducesResponseType(typeof(AgentTaskStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AgentTaskStatus>> GetTaskStatus(
        [FromRoute] string taskId,
        CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation("Getting task status for {TaskId}, user {UserId}", taskId, userId);

        try
        {
            var status = await _agentService.GetTaskStatusAsync(taskId, cancellationToken);

            if (status.Status == "error" && status.Error?.Contains("not found") == true)
            {
                return NotFound(new { Error = "Task not found", TaskId = taskId });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task status for {TaskId}", taskId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Error = "Failed to get task status",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Stream real-time updates for a queued agent task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SSE stream of task updates</returns>
    [HttpGet("tasks/{taskId}/stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task StreamTaskUpdates(
        [FromRoute] string taskId,
        CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        _logger.LogInformation(
            "Streaming task updates for {TaskId}, user {UserId}",
            taskId, userId);

        // Set SSE headers
        Response.ContentType = "text/event-stream";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        Response.Headers.Add("X-Accel-Buffering", "no"); // Disable nginx buffering

        try
        {
            await foreach (var streamEvent in _agentService.StreamTaskUpdatesAsync(taskId, userId, cancellationToken))
            {
                try
                {
                    // Serialize and send the event
                    var eventData = System.Text.Json.JsonSerializer.Serialize(streamEvent);
                    await Response.WriteAsync($"event: {streamEvent.Type}\n", cancellationToken);
                    await Response.WriteAsync($"data: {eventData}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);

                    _logger.LogDebug(
                        "Sent stream event {EventType} for task {TaskId}",
                        streamEvent.Type, taskId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error writing stream event to client for task {TaskId}", taskId);
                    break;
                }
            }

            _logger.LogInformation("Streaming completed for task {TaskId}", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during streaming for task {TaskId}", taskId);

            try
            {
                var errorEvent = new { type = "error", message = ex.Message };
                var errorData = System.Text.Json.JsonSerializer.Serialize(errorEvent);
                await Response.WriteAsync($"event: error\n", cancellationToken);
                await Response.WriteAsync($"data: {errorData}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
            catch
            {
                // Client likely disconnected
            }
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

/// <summary>
/// Response model for queuing a task
/// </summary>
public record QueueTaskResponse
{
    public string TaskId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
