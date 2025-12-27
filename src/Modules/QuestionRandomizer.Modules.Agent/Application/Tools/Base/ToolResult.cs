namespace QuestionRandomizer.Modules.Agent.Application.Tools.Base;

using System.Text.Json;

/// <summary>
/// Result of a tool execution
/// </summary>
public class ToolResult
{
    /// <summary>
    /// Whether the tool execution was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Result content (serialized to JSON for the agent)
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Error message if the execution failed
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Creates a successful tool result
    /// </summary>
    public static ToolResult SuccessResult(object data)
    {
        return new ToolResult
        {
            Success = true,
            Content = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            })
        };
    }

    /// <summary>
    /// Creates a failed tool result
    /// </summary>
    public static ToolResult ErrorResult(string error)
    {
        return new ToolResult
        {
            Success = false,
            Content = string.Empty,
            Error = error
        };
    }

    /// <summary>
    /// Creates a failed tool result from an exception
    /// </summary>
    public static ToolResult ErrorResult(Exception ex)
    {
        return new ToolResult
        {
            Success = false,
            Content = string.Empty,
            Error = $"{ex.GetType().Name}: {ex.Message}"
        };
    }
}
