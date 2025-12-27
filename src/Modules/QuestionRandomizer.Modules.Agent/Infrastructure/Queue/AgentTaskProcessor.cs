namespace QuestionRandomizer.Modules.Agent.Infrastructure.Queue;

using Hangfire;
using Microsoft.Extensions.Logging;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Conversations.Domain.Entities;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;

/// <summary>
/// Background processor for agent tasks with Firestore-backed status tracking
/// Executed by Hangfire workers
/// </summary>
public class AgentTaskProcessor
{
    private readonly IAgentExecutor _agentExecutor;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<AgentTaskProcessor> _logger;

    public AgentTaskProcessor(
        IAgentExecutor agentExecutor,
        IAgentTaskRepository taskRepository,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ILogger<AgentTaskProcessor> logger)
    {
        _agentExecutor = agentExecutor;
        _taskRepository = taskRepository;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    /// <summary>
    /// Processes an agent task in the background with status updates
    /// This method is called by Hangfire
    /// </summary>
    /// <param name="taskId">Unique task identifier</param>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="conversationId">Optional conversation ID for context continuity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 5, 15, 30 })]
    public async Task ProcessTaskAsync(
        string taskId,
        string task,
        string userId,
        string? conversationId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing background agent task {TaskId} for user {UserId} (ConversationId: {ConversationId})",
                taskId, userId, conversationId ?? "none");

            // Update status to processing
            await _taskRepository.UpdateStatusAsync(
                taskId,
                userId,
                "processing",
                DateTime.UtcNow,
                cancellationToken);

            // Load conversation history if conversationId is provided
            List<ConversationMessage>? conversationHistory = null;
            string? activeConversationId = conversationId;

            if (!string.IsNullOrEmpty(conversationId))
            {
                var messages = await _messageRepository.GetByConversationIdAsync(conversationId, cancellationToken);
                conversationHistory = messages
                    .Select(m => new ConversationMessage { Role = m.Role, Content = m.Content })
                    .ToList();

                _logger.LogInformation(
                    "Loaded {Count} messages from conversation {ConversationId}",
                    conversationHistory.Count, conversationId);
            }
            else
            {
                // Create a new conversation
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

            // Save user message
            await _messageRepository.CreateAsync(
                new Message
                {
                    ConversationId = activeConversationId!,
                    Role = "user",
                    Content = task
                },
                cancellationToken);

            // Execute the agent task with conversation history
            var result = await _agentExecutor.ExecuteTaskAsync(task, userId, conversationHistory, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Background agent task {TaskId} completed successfully",
                    taskId);

                // Save agent response
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

                // Store result in Firestore
                await _taskRepository.SetResultAsync(taskId, userId, result.Result, cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "Background agent task {TaskId} failed: {Error}",
                    taskId, result.Error);

                // Store error in Firestore
                await _taskRepository.SetErrorAsync(
                    taskId,
                    userId,
                    result.Error ?? "Unknown error",
                    cancellationToken);

                throw new InvalidOperationException(
                    $"Agent task failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing background agent task {TaskId}",
                taskId);

            // Store error in Firestore
            try
            {
                await _taskRepository.SetErrorAsync(
                    taskId,
                    userId,
                    ex.Message,
                    cancellationToken);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(
                    innerEx,
                    "Failed to store error status for task {TaskId}",
                    taskId);
            }

            // Hangfire will retry based on [AutomaticRetry] attribute
            throw;
        }
    }
}
