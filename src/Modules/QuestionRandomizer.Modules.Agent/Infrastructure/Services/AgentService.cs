namespace QuestionRandomizer.Modules.Agent.Infrastructure.Services;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;
using QuestionRandomizer.Modules.Conversations.Domain.Entities;

/// <summary>
/// Service for executing AI agent tasks with real-time streaming
/// Provides ChatGPT-like conversational experience
/// </summary>
public class AgentService : IAgentService
{
    private readonly IAgentExecutor _agentExecutor;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<AgentService> _logger;

    public AgentService(
        IAgentExecutor agentExecutor,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ILogger<AgentService> logger)
    {
        _agentExecutor = agentExecutor;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async IAsyncEnumerable<AgentStreamEvent> ExecuteTaskStreamingAsync(
        string task,
        string userId,
        string? conversationId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting streaming execution for user {UserId} (ConversationId: {ConversationId})",
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

        // Stream execution events from agent
        var responseBuilder = new System.Text.StringBuilder();

        await foreach (var streamEvent in _agentExecutor.ExecuteTaskStreamingAsync(
            task, userId, conversationHistory, cancellationToken))
        {
            // Collect text chunks for conversation storage
            if (streamEvent.Type == "text_chunk" && !string.IsNullOrEmpty(streamEvent.Content))
            {
                responseBuilder.AppendLine(streamEvent.Content);
            }

            // Yield event to client
            yield return streamEvent;

            // If completed, save assistant response
            if (streamEvent.Type == "completed")
            {
                var assistantResponse = !string.IsNullOrEmpty(streamEvent.Content)
                    ? streamEvent.Content
                    : responseBuilder.ToString();

                if (!string.IsNullOrEmpty(assistantResponse))
                {
                    await _messageRepository.CreateAsync(
                        new Message
                        {
                            ConversationId = activeConversationId!,
                            Role = "assistant",
                            Content = assistantResponse
                        },
                        cancellationToken);

                    // Update conversation timestamp
                    await _conversationRepository.UpdateTimestampAsync(activeConversationId!, userId, cancellationToken);
                }
            }
        }

        _logger.LogInformation("Streaming execution completed for user {UserId}", userId);
    }
}
