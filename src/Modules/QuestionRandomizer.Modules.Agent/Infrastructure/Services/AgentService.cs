namespace QuestionRandomizer.Modules.Agent.Infrastructure.Services;

using System.Runtime.CompilerServices;
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

        _logger.LogInformation(
            "Agent task {TaskId} {Status}",
            result.TaskId,
            result.Success ? "completed successfully" : "failed");

        return result;
    }

    public async Task<AgentTaskStatus> GetTaskStatusAsync(
        string taskId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting status for task {TaskId}, user {UserId}", taskId, userId);

        // NOTE: This method is designed for queued tasks only.
        // For synchronous tasks (POST /api/agent/execute), the result is returned immediately,
        // so there's no need to query status afterward.

        // Get task from Firestore (queued tasks are stored there)
        var agentTask = await _taskQueueService.GetTaskWithUserIdAsync(taskId, userId, cancellationToken);

        if (agentTask == null)
        {
            // Task not found or user doesn't have access - return unknown status
            return new AgentTaskStatus
            {
                TaskId = taskId,
                Status = "unknown",
                Error = "Task not found"
            };
        }

        // Map AgentTask to AgentTaskStatus
        return new AgentTaskStatus
        {
            TaskId = agentTask.TaskId,
            Status = agentTask.Status,
            Result = agentTask.Result,
            Error = agentTask.Error,
            CreatedAt = agentTask.CreatedAt,
            CompletedAt = agentTask.CompletedAt
        };
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

        _logger.LogInformation("Agent task queued with ID {TaskId}", taskId);

        return taskId;
    }

    public async IAsyncEnumerable<AgentStreamEvent> StreamTaskUpdatesAsync(
        string taskId,
        string userId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting stream for task {TaskId}, user {UserId}",
            taskId, userId);

        var lastStatus = string.Empty;
        var pollInterval = 1000; // Start with 1 second
        const int maxPollInterval = 3000; // Max 3 seconds

        // Send initial status event
        yield return new AgentStreamEvent
        {
            Type = "started",
            Message = "Waiting for task to begin...",
            Timestamp = DateTime.UtcNow
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            // Get current task status from Firestore
            var agentTask = await _taskQueueService.GetTaskWithUserIdAsync(
                taskId, userId, cancellationToken);

            if (agentTask == null)
            {
                yield return new AgentStreamEvent
                {
                    Type = "error",
                    Message = "Task not found",
                    Timestamp = DateTime.UtcNow
                };
                yield break;
            }

            // Send update if status changed
            if (agentTask.Status != lastStatus)
            {
                lastStatus = agentTask.Status;

                yield return new AgentStreamEvent
                {
                    Type = "status_change",
                    Message = $"Status: {agentTask.Status}",
                    Output = agentTask.Status,
                    Timestamp = DateTime.UtcNow
                };

                // Reset to faster polling on status change
                pollInterval = 1000;
            }

            // Check if task is in terminal state
            if (agentTask.Status == "completed")
            {
                yield return new AgentStreamEvent
                {
                    Type = "completed",
                    Message = "Task completed successfully",
                    Content = agentTask.Result,
                    Output = agentTask.Result,
                    Timestamp = DateTime.UtcNow
                };
                yield break;
            }

            if (agentTask.Status == "failed")
            {
                yield return new AgentStreamEvent
                {
                    Type = "error",
                    Message = agentTask.Error ?? "Task failed",
                    Output = agentTask.Error,
                    Timestamp = DateTime.UtcNow
                };
                yield break;
            }

            // Wait before next poll (exponential backoff)
            await Task.Delay(pollInterval, cancellationToken);
            pollInterval = Math.Min((int)(pollInterval * 1.5), maxPollInterval);
        }

        _logger.LogInformation(
            "Stream cancelled for task {TaskId}",
            taskId);
    }
}
