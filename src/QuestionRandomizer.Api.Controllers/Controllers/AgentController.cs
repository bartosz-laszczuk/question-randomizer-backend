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
