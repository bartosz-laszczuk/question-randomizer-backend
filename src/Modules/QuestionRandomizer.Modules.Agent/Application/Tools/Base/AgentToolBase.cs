namespace QuestionRandomizer.Modules.Agent.Application.Tools.Base;

using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;

/// <summary>
/// Abstract base class for all agent tools
/// Provides common functionality for validation, error handling, and execution
/// </summary>
/// <typeparam name="TInput">The input type for this tool (will be deserialized from JSON)</typeparam>
public abstract class AgentToolBase<TInput> : IAgentTool
    where TInput : class
{
    protected ILogger Logger { get; }

    protected AgentToolBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// The name of the tool (override in derived classes)
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Description of what the tool does (override in derived classes)
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// JSON schema for the input parameters (override in derived classes)
    /// </summary>
    public abstract string InputSchemaJson { get; }

    /// <summary>
    /// Validates and parses the JSON input (override to add validation)
    /// </summary>
    protected abstract TInput ValidateAndParse(JsonElement input);

    /// <summary>
    /// Executes the tool logic (override in derived classes)
    /// IMPORTANT: Always filter queries by userId for security
    /// </summary>
    /// <param name="input">Validated and parsed input</param>
    /// <param name="userId">User ID for security filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result object to be serialized to JSON</returns>
    protected abstract Task<object> ExecuteToolAsync(
        TInput input,
        string userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Template method that implements the execution flow
    /// </summary>
    public async Task<ToolResult> ExecuteAsync(
        JsonElement input,
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation(
                "Executing tool {ToolName} for user {UserId}",
                Name, userId);

            // 1. Validate and parse input
            var validated = ValidateAndParse(input);

            // 2. Execute tool logic
            var result = await ExecuteToolAsync(validated, userId, cancellationToken);

            Logger.LogInformation(
                "Tool {ToolName} executed successfully for user {UserId}",
                Name, userId);

            // 3. Return success result
            return ToolResult.SuccessResult(result);
        }
        catch (ValidationException ex)
        {
            Logger.LogWarning(
                ex,
                "Validation failed for tool {ToolName}: {Errors}",
                Name, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return ToolResult.ErrorResult($"Validation error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Logger.LogWarning(
                ex,
                "JSON parsing failed for tool {ToolName}",
                Name);

            return ToolResult.ErrorResult($"Invalid JSON input: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Error executing tool {ToolName} for user {UserId}",
                Name, userId);

            return ToolResult.ErrorResult(ex);
        }
    }

    /// <summary>
    /// Helper method to deserialize and validate input using FluentValidation
    /// </summary>
    protected TInput DeserializeAndValidate(
        JsonElement input,
        IValidator<TInput> validator)
    {
        // Deserialize
        var deserialized = JsonSerializer.Deserialize<TInput>(input.GetRawText(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (deserialized == null)
        {
            throw new JsonException("Failed to deserialize input");
        }

        // Validate
        validator.ValidateAndThrow(deserialized);

        return deserialized;
    }
}
