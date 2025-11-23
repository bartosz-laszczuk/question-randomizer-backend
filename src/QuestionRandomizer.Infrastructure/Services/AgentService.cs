namespace QuestionRandomizer.Infrastructure.Services;

using QuestionRandomizer.Application.Interfaces;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for communicating with the TypeScript Agent Service
/// </summary>
public class AgentService : IAgentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgentService> _logger;

    public AgentService(HttpClient httpClient, ILogger<AgentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AgentTaskResult> ExecuteTaskAsync(string task, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing agent task for user {UserId}", userId);

            var request = new
            {
                task,
                userId
            };

            var response = await _httpClient.PostAsJsonAsync("/agent/task", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AgentTaskResultDto>(cancellationToken);

            if (result == null)
            {
                return new AgentTaskResult
                {
                    TaskId = string.Empty,
                    Success = false,
                    Error = "Failed to parse agent response",
                    Result = string.Empty
                };
            }

            _logger.LogInformation("Agent task {TaskId} completed successfully", result.TaskId);

            return new AgentTaskResult
            {
                TaskId = result.TaskId,
                Success = result.Success,
                Result = result.Result ?? string.Empty,
                Error = result.Error
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error communicating with agent service");
            return new AgentTaskResult
            {
                TaskId = string.Empty,
                Success = false,
                Error = $"Agent service communication error: {ex.Message}",
                Result = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing agent task");
            return new AgentTaskResult
            {
                TaskId = string.Empty,
                Success = false,
                Error = $"Unexpected error: {ex.Message}",
                Result = string.Empty
            };
        }
    }

    public async Task<AgentTaskStatus> GetTaskStatusAsync(string taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting status for task {TaskId}", taskId);

            var response = await _httpClient.GetAsync($"/agent/task/{taskId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var status = await response.Content.ReadFromJsonAsync<AgentTaskStatusDto>(cancellationToken);

            if (status == null)
            {
                return new AgentTaskStatus
                {
                    TaskId = taskId,
                    Status = "error",
                    Error = "Failed to parse agent response"
                };
            }

            return new AgentTaskStatus
            {
                TaskId = status.TaskId,
                Status = status.Status,
                Result = status.Result,
                Error = status.Error
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error getting task status for {TaskId}", taskId);
            return new AgentTaskStatus
            {
                TaskId = taskId,
                Status = "error",
                Error = $"Agent service communication error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting task status for {TaskId}", taskId);
            return new AgentTaskStatus
            {
                TaskId = taskId,
                Status = "error",
                Error = $"Unexpected error: {ex.Message}"
            };
        }
    }

    // Internal DTOs for deserialization
    private record AgentTaskResultDto
    {
        public string TaskId { get; init; } = string.Empty;
        public bool Success { get; init; }
        public string? Result { get; init; }
        public string? Error { get; init; }
    }

    private record AgentTaskStatusDto
    {
        public string TaskId { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? Result { get; init; }
        public string? Error { get; init; }
    }
}
