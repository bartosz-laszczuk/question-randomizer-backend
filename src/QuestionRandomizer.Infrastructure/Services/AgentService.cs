namespace QuestionRandomizer.Infrastructure.Services;

using QuestionRandomizer.Application.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for communicating with the TypeScript Agent Service
/// </summary>
public class AgentService : IAgentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgentService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AgentService(HttpClient httpClient, ILogger<AgentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
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

            var status = await response.Content.ReadFromJsonAsync<AgentTaskStatusDto>(_jsonOptions, cancellationToken);

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
                Error = status.Error,
                CreatedAt = status.CreatedAt,
                CompletedAt = status.CompletedAt,
                Metadata = status.Metadata
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

    public async Task<AgentTaskResult> ExecuteTaskStreamingAsync(
        string task,
        string userId,
        Action<AgentStreamEvent> onProgress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing streaming agent task for user {UserId}", userId);

            var request = new
            {
                task,
                userId
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/agent/task/stream")
            {
                Content = JsonContent.Create(request)
            };

            // Send request and get response stream
            var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            string? finalTaskId = null;
            string? finalResult = null;

            // Read SSE stream
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                    continue;

                // Parse SSE format: "event: <type>\ndata: <json>\n"
                if (line.StartsWith("event:"))
                {
                    var eventType = line.Substring(6).Trim();
                    var dataLine = await reader.ReadLineAsync();

                    if (dataLine?.StartsWith("data:") == true)
                    {
                        var jsonData = dataLine.Substring(5).Trim();
                        var eventData = ParseStreamEvent(eventType, jsonData);

                        if (eventData != null)
                        {
                            onProgress(eventData);

                            // Capture final result
                            if (eventType == "complete" && eventData.Content != null)
                            {
                                var completeData = JsonSerializer.Deserialize<CompleteEventData>(jsonData, _jsonOptions);
                                finalTaskId = completeData?.TaskId;
                                finalResult = completeData?.Result;
                            }
                        }
                    }
                }
            }

            _logger.LogInformation("Streaming agent task completed for user {UserId}", userId);

            return new AgentTaskResult
            {
                TaskId = finalTaskId ?? string.Empty,
                Success = true,
                Result = finalResult ?? "Task completed",
                Error = null
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error communicating with agent service during streaming");
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
            _logger.LogError(ex, "Unexpected error executing streaming agent task");
            return new AgentTaskResult
            {
                TaskId = string.Empty,
                Success = false,
                Error = $"Unexpected error: {ex.Message}",
                Result = string.Empty
            };
        }
    }

    public async Task<string> QueueTaskAsync(string task, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Queueing agent task for user {UserId}", userId);

            var request = new
            {
                task,
                userId
            };

            var response = await _httpClient.PostAsJsonAsync("/agent/task/queue", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<QueueTaskResponseDto>(_jsonOptions, cancellationToken);

            if (result == null || string.IsNullOrEmpty(result.TaskId))
            {
                throw new Exception("Failed to parse queue response");
            }

            _logger.LogInformation("Agent task queued with ID {TaskId}", result.TaskId);

            return result.TaskId;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error queuing agent task");
            throw new Exception($"Agent service communication error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error queuing agent task");
            throw;
        }
    }

    private AgentStreamEvent? ParseStreamEvent(string eventType, string jsonData)
    {
        try
        {
            var baseData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData, _jsonOptions);
            if (baseData == null)
                return null;

            var streamEvent = new AgentStreamEvent
            {
                Type = eventType
            };

            // Parse based on event type
            switch (eventType)
            {
                case "progress":
                    return streamEvent with
                    {
                        Message = baseData.GetValueOrDefault("message").GetString(),
                        Progress = baseData.ContainsKey("progress") ? baseData["progress"].GetInt32() : null
                    };

                case "thinking":
                    return streamEvent with
                    {
                        Content = baseData.GetValueOrDefault("content").GetString()
                    };

                case "tool_use":
                    return streamEvent with
                    {
                        ToolName = baseData.GetValueOrDefault("toolName").GetString(),
                        Input = baseData.ContainsKey("input") ? baseData["input"] : null,
                        Output = baseData.GetValueOrDefault("output").GetString()
                    };

                case "complete":
                    return streamEvent with
                    {
                        Content = baseData.GetValueOrDefault("result").GetString()
                    };

                case "error":
                    return streamEvent with
                    {
                        Message = baseData.GetValueOrDefault("message").GetString()
                    };

                default:
                    return streamEvent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse stream event: {EventType}", eventType);
            return null;
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
        public DateTime? CreatedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public AgentTaskMetadata? Metadata { get; init; }
    }

    private record QueueTaskResponseDto
    {
        public bool Success { get; init; }
        public string TaskId { get; init; } = string.Empty;
        public string JobId { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? Message { get; init; }
    }

    private record CompleteEventData
    {
        public string TaskId { get; init; } = string.Empty;
        public string? Result { get; init; }
        public AgentTaskMetadata? Metadata { get; init; }
    }
}
