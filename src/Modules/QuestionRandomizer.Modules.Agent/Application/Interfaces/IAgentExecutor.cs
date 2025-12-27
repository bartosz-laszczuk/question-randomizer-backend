namespace QuestionRandomizer.Modules.Agent.Application.Interfaces;

using QuestionRandomizer.Modules.Agent.Application.DTOs;

/// <summary>
/// Service for executing AI agent tasks using Claude with tool calling
/// </summary>
public interface IAgentExecutor
{
    /// <summary>
    /// Executes an agent task synchronously (waits for completion)
    /// The agent will autonomously use tools to complete the task
    /// </summary>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">The user ID making the request (used for security filtering in tools)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the agent task execution</returns>
    Task<AgentTaskResult> ExecuteTaskAsync(
        string task,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an agent task with streaming progress updates
    /// </summary>
    /// <param name="task">The task description for the agent</param>
    /// <param name="userId">The user ID making the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of progress events</returns>
    IAsyncEnumerable<AgentStreamEvent> ExecuteTaskStreamingAsync(
        string task,
        string userId,
        CancellationToken cancellationToken = default);
}
