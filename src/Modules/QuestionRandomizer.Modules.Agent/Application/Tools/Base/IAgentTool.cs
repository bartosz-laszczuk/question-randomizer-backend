namespace QuestionRandomizer.Modules.Agent.Application.Tools.Base;

using System.Text.Json;

/// <summary>
/// Interface for agent tools that can be called by the AI agent
/// </summary>
public interface IAgentTool
{
    /// <summary>
    /// The name of the tool (used by the agent to identify and call it)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of what the tool does (helps the agent decide when to use it)
    /// </summary>
    string Description { get; }

    /// <summary>
    /// JSON schema defining the input parameters for this tool
    /// </summary>
    string InputSchemaJson { get; }

    /// <summary>
    /// Executes the tool with the given input
    /// </summary>
    /// <param name="input">JSON input parameters for the tool</param>
    /// <param name="userId">User ID for security filtering (CRITICAL: always filter by userId)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the tool execution</returns>
    Task<ToolResult> ExecuteAsync(
        JsonElement input,
        string userId,
        CancellationToken cancellationToken = default);
}
