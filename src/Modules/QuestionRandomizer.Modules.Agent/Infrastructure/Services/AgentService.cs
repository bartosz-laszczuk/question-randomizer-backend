namespace QuestionRandomizer.Modules.Agent.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Infrastructure.Queue;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;
using QuestionRandomizer.Modules.Conversations.Domain.Entities;

/// <summary>
/// Service for executing agent tasks using the integrated .NET AI agent
/// Replaced the HTTP client to TypeScript agent service
/// </summary>
public class AgentService : IAgentService
{
    private readonly IAgentExecutor _agentExecutor;
    private readonly ITaskQueueService _taskQueueService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<AgentService> _logger;

    // In-memory task status tracking (simplified for now; could use cache/database in production)
    private readonly Dictionary<string, AgentTaskStatus> _taskStatuses = new();

    public AgentService(
        IAgentExecutor agentExecutor,
        ITaskQueueService taskQueueService,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ILogger<AgentService> logger)
    {
        _agentExecutor = agentExecutor;
        _taskQueueService = taskQueueService;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task<AgentTaskResult> ExecuteTaskAsync(
        string task,
        string userId,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Executing agent task for user {UserId} (ConversationId: {ConversationId})",
            userId, conversationId ?? "none");

        // Load conversation history if conversationId is provided
        List<ConversationMessage>? conversationHistory = null;
        string? activeConversationId = conversationId;

        if (!string.IsNullOrEmpty(conversationId))
        {
            // Fetch existing conversation and its messages
            var messages = await _messageRepository.GetByConversationIdAsync(conversationId, cancellationToken);
            conversationHistory = messages
                .Select(m => new ConversationMessage
                {
                    Role = m.Role,
                    Content = m.Content
                })
                .ToList();

            _logger.LogInformation(
                "Loaded {Count} messages from conversation {ConversationId}",
                conversationHistory.Count, conversationId);
        }
        else
        {
            // Create a new conversation for this task
            var conversation = await _conversationRepository.CreateAsync(
                new Conversation
                {
                    UserId = userId,
                    Title = task.Length > 50 ? task.Substring(0, 47) + "..." : task,
                    IsActive = true
                },
                cancellationToken);

            activeConversationId = conversation.Id;
            _logger.LogInformation("Created new conversation {ConversationId}", activeConversationId);
        }

        // Save user message to conversation
        await _messageRepository.CreateAsync(
            new Message
            {
                ConversationId = activeConversationId!,
                Role = "user",
                Content = task
            },
            cancellationToken);

        // Execute agent with conversation history
        var result = await _agentExecutor.ExecuteTaskAsync(task, userId, conversationHistory, cancellationToken);

        // Save agent response to conversation
        if (result.Success)
        {
            await _messageRepository.CreateAsync(
                new Message
                {
                    ConversationId = activeConversationId!,
                    Role = "assistant",
                    Content = result.Result
                },
                cancellationToken);

            // Update conversation timestamp
            await _conversationRepository.UpdateTimestampAsync(activeConversationId!, userId, cancellationToken);
        }

        // Store task status
        _taskStatuses[result.TaskId] = new AgentTaskStatus
        {
            TaskId = result.TaskId,
            Status = result.Success ? "completed" : "failed",
            Result = result.Result,
            Error = result.Error,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Agent task {TaskId} {Status}",
            result.TaskId,
            result.Success ? "completed successfully" : "failed");

        return result;
    }

    public Task<AgentTaskStatus> GetTaskStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting status for task {TaskId}", taskId);

        if (_taskStatuses.TryGetValue(taskId, out var status))
        {
            return Task.FromResult(status);
        }

        // Task not found - return unknown status
        return Task.FromResult(new AgentTaskStatus
        {
            TaskId = taskId,
            Status = "unknown",
            Error = "Task not found"
        });
    }

    public async Task<AgentTaskResult> ExecuteTaskStreamingAsync(
        string task,
        string userId,
        Action<AgentStreamEvent> onProgress,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Executing streaming agent task for user {UserId} (ConversationId: {ConversationId})",
            userId, conversationId ?? "none");

        // Load conversation history if conversationId is provided
        List<ConversationMessage>? conversationHistory = null;
        string? activeConversationId = conversationId;

        if (!string.IsNullOrEmpty(conversationId))
        {
            var messages = await _messageRepository.GetByConversationIdAsync(conversationId, cancellationToken);
            conversationHistory = messages
                .Select(m => new ConversationMessage { Role = m.Role, Content = m.Content })
                .ToList();
        }
        else
        {
            var conversation = await _conversationRepository.CreateAsync(
                new Conversation
                {
                    UserId = userId,
                    Title = task.Length > 50 ? task.Substring(0, 47) + "..." : task,
                    IsActive = true
                },
                cancellationToken);

            activeConversationId = conversation.Id;
        }

        // Save user message
        await _messageRepository.CreateAsync(
            new Message
            {
                ConversationId = activeConversationId!,
                Role = "user",
                Content = task
            },
            cancellationToken);

        var finalResult = new AgentTaskResult
        {
            TaskId = Guid.NewGuid().ToString(),
            Success = true,
            Result = string.Empty,
            Error = null
        };

        try
        {
            // Execute task with streaming
            await foreach (var streamEvent in _agentExecutor.ExecuteTaskStreamingAsync(task, userId, conversationHistory, cancellationToken))
            {
                // Forward progress events to caller
                onProgress(streamEvent);

                // Capture final result
                if (streamEvent.Type == "completed" && !string.IsNullOrEmpty(streamEvent.Content))
                {
                    finalResult = finalResult with
                    {
                        Success = true,
                        Result = streamEvent.Content
                    };
                }
                else if (streamEvent.Type == "error")
                {
                    finalResult = finalResult with
                    {
                        Success = false,
                        Error = streamEvent.Message ?? streamEvent.Output
                    };
                }
            }

            // Save agent response
            if (finalResult.Success)
            {
                await _messageRepository.CreateAsync(
                    new Message
                    {
                        ConversationId = activeConversationId!,
                        Role = "assistant",
                        Content = finalResult.Result
                    },
                    cancellationToken);

                await _conversationRepository.UpdateTimestampAsync(activeConversationId!, userId, cancellationToken);
            }

            // Store task status
            _taskStatuses[finalResult.TaskId] = new AgentTaskStatus
            {
                TaskId = finalResult.TaskId,
                Status = finalResult.Success ? "completed" : "failed",
                Result = finalResult.Result,
                Error = finalResult.Error,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Streaming agent task completed for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during streaming agent task execution");
            finalResult = finalResult with
            {
                Success = false,
                Error = $"Unexpected error: {ex.Message}"
            };
        }

        return finalResult;
    }

    public async Task<string> QueueTaskAsync(
        string task,
        string userId,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Queueing agent task for user {UserId} (ConversationId: {ConversationId})",
            userId, conversationId ?? "none");

        var taskId = await _taskQueueService.QueueTaskAsync(task, userId, conversationId, cancellationToken);

        // Store initial task status
        _taskStatuses[taskId] = new AgentTaskStatus
        {
            TaskId = taskId,
            Status = "queued",
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Agent task queued with ID {TaskId}", taskId);

        return taskId;
    }
}
