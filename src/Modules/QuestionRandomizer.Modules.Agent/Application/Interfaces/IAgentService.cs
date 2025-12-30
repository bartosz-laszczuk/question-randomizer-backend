namespace QuestionRandomizer.Modules.Agent.Application.Interfaces;

using System.Runtime.CompilerServices;
using QuestionRandomizer.Modules.Agent.Application.DTOs;

/// <summary>
/// Service for executing AI agent tasks with real-time streaming
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Executes an agent task with real-time streaming (ChatGPT-like behavior)
    /// </summary>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">The user ID making the request</param>
    /// <param name="conversationId">Optional conversation ID for context continuity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async stream of agent execution events (text chunks, tool calls, etc.)</returns>
    IAsyncEnumerable<AgentStreamEvent> ExecuteTaskStreamingAsync(
        string task,
        string userId,
        string? conversationId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
}
