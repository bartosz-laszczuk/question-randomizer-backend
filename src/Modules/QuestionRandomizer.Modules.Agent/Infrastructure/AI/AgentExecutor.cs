namespace QuestionRandomizer.Modules.Agent.Infrastructure.AI;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic.SDK;
using Anthropic.SDK.Common;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestionRandomizer.Modules.Agent.Application.DTOs;
using QuestionRandomizer.Modules.Agent.Application.Interfaces;
using QuestionRandomizer.Modules.Agent.Application.Tools;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;
using CommonTool = Anthropic.SDK.Common.Tool;

/// <summary>
/// Executes AI agent tasks using Claude with autonomous tool calling
/// Replaces the TypeScript agent-executor.ts
/// </summary>
public class AgentExecutor : IAgentExecutor
{
    private readonly AnthropicClient _anthropicClient;
    private readonly AgentConfiguration _config;
    private readonly ToolRegistry _toolRegistry;
    private readonly ILogger<AgentExecutor> _logger;

    public AgentExecutor(
        IOptions<AgentConfiguration> config,
        ToolRegistry toolRegistry,
        ILogger<AgentExecutor> logger)
    {
        _config = config.Value;
        _toolRegistry = toolRegistry;
        _logger = logger;

        // Initialize Anthropic client
        _anthropicClient = new AnthropicClient(new APIAuthentication(_config.ApiKey));
    }

    public async Task<AgentTaskResult> ExecuteTaskAsync(
        string task,
        string userId,
        List<ConversationMessage>? conversationHistory = null,
        CancellationToken cancellationToken = default)
    {
        var taskId = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation(
                "Starting agent task {TaskId} for user {UserId}: {Task} (Conversation history: {HasHistory})",
                taskId, userId, task, conversationHistory?.Count > 0);

            // Wrap execution with timeout protection
            var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_config.TimeoutSeconds));

            var executionTask = ExecuteAgentLoopAsync(taskId, task, userId, conversationHistory, timeoutCts.Token);
            var completedTask = await Task.WhenAny(
                executionTask,
                Task.Delay(TimeSpan.FromSeconds(_config.TimeoutSeconds), cancellationToken));

            if (completedTask != executionTask)
            {
                _logger.LogWarning(
                    "Agent task {TaskId} exceeded timeout of {TimeoutSeconds} seconds",
                    taskId, _config.TimeoutSeconds);

                return new AgentTaskResult
                {
                    TaskId = taskId,
                    Success = false,
                    Result = string.Empty,
                    Error = $"Agent task exceeded timeout of {_config.TimeoutSeconds} seconds"
                };
            }

            return await executionTask;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Agent task {TaskId} was cancelled", taskId);
            return new AgentTaskResult
            {
                TaskId = taskId,
                Success = false,
                Result = string.Empty,
                Error = "Agent task was cancelled"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error executing agent task {TaskId} for user {UserId}",
                taskId, userId);

            return new AgentTaskResult
            {
                TaskId = taskId,
                Success = false,
                Result = string.Empty,
                Error = $"Agent execution failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Executes the agent loop with tool calling
    /// </summary>
    private async Task<AgentTaskResult> ExecuteAgentLoopAsync(
        string taskId,
        string task,
        string userId,
        List<ConversationMessage>? conversationHistory,
        CancellationToken cancellationToken)
    {
        try
        {
            // Build tool definitions
            var toolDefinitions = BuildToolDefinitions();

            // Initialize conversation with history (if provided) or start fresh
            var messages = new List<Message>();

            // Add conversation history if provided
            if (conversationHistory != null && conversationHistory.Count > 0)
            {
                foreach (var msg in conversationHistory)
                {
                    messages.Add(new Message
                    {
                        Role = msg.Role.ToLower() == "user" ? RoleType.User : RoleType.Assistant,
                        Content = new List<ContentBase>
                        {
                            new TextContent { Text = msg.Content }
                        }
                    });
                }

                _logger.LogDebug("Loaded {Count} messages from conversation history", conversationHistory.Count);
            }

            // Add current task as new user message
            messages.Add(new Message
            {
                Role = RoleType.User,
                Content = new List<ContentBase>
                {
                    new TextContent { Text = task }
                }
            });

            // Agent loop - iterate until task completion or max iterations
            var iteration = 0;
            var finalTextResponse = new System.Text.StringBuilder();

            while (iteration < _config.MaxIterations)
            {
                iteration++;
                _logger.LogDebug("Agent iteration {Iteration}/{MaxIterations}", iteration, _config.MaxIterations);

                // Call Claude with tools
                var parameters = new MessageParameters
                {
                    Messages = messages,
                    MaxTokens = _config.MaxTokens,
                    Model = _config.Model,
                    Temperature = (decimal)_config.Temperature,
                    System = new List<SystemMessage>
                    {
                        new(_config.SystemPrompt)
                    },
                    Tools = toolDefinitions.Cast<CommonTool>().ToList()
                };

                var response = await _anthropicClient.Messages.GetClaudeMessageAsync(
                    parameters,
                    cancellationToken);

                // Check stop reason
                if (response.StopReason == "end_turn")
                {
                    // Agent finished - extract text response
                    var textContent = response.Content?
                        .Where(c => c is TextContent)
                        .Cast<TextContent>()
                        .Select(c => c.Text)
                        .ToList();

                    if (textContent?.Any() == true)
                    {
                        finalTextResponse.AppendLine(string.Join("\n", textContent));
                    }

                    _logger.LogInformation("Agent task {TaskId} completed successfully after {Iterations} iterations",
                        taskId, iteration);

                    break;
                }

                if (response.StopReason == "tool_use")
                {
                    // Agent wants to use tools
                    var toolUses = response.Content?
                        .Where(c => c is ToolUseContent)
                        .Cast<ToolUseContent>()
                        .ToList() ?? new List<ToolUseContent>();

                    if (!toolUses.Any())
                    {
                        _logger.LogWarning("Stop reason was tool_use but no tool use content found");
                        break;
                    }

                    // Collect text from this turn
                    var textContent = response.Content?
                        .Where(c => c is TextContent)
                        .Cast<TextContent>()
                        .Select(c => c.Text)
                        .ToList();

                    if (textContent?.Any() == true)
                    {
                        finalTextResponse.AppendLine(string.Join("\n", textContent));
                    }

                    // Add assistant message to conversation
                    messages.Add(new Message
                    {
                        Role = RoleType.Assistant,
                        Content = response.Content
                    });

                    // Execute tools and collect results
                    var toolResults = await ExecuteToolsAsync(toolUses, userId, cancellationToken);

                    // Add tool results as user message
                    messages.Add(new Message
                    {
                        Role = RoleType.User,
                        Content = toolResults.Select(tr => new ToolResultContent
                        {
                            ToolUseId = tr.ToolUseId,
                            Content = new List<ContentBase>
                            {
                                new TextContent { Text = tr.Content }
                            }
                        }).ToList<ContentBase>()
                    });

                    // Continue to next iteration
                    continue;
                }

                // Unexpected stop reason
                _logger.LogWarning("Unexpected stop reason: {StopReason}", response.StopReason);
                break;
            }

            if (iteration >= _config.MaxIterations)
            {
                _logger.LogWarning("Agent task {TaskId} reached max iterations ({MaxIterations})",
                    taskId, _config.MaxIterations);
            }

            return new AgentTaskResult
            {
                TaskId = taskId,
                Success = true,
                Result = finalTextResponse.ToString(),
                Error = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error executing agent loop for task {TaskId}",
                taskId);

            return new AgentTaskResult
            {
                TaskId = taskId,
                Success = false,
                Result = string.Empty,
                Error = $"Agent execution failed: {ex.Message}"
            };
        }
    }

    public async IAsyncEnumerable<AgentStreamEvent> ExecuteTaskStreamingAsync(
        string task,
        string userId,
        List<ConversationMessage>? conversationHistory = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var taskId = Guid.NewGuid().ToString();

        yield return new AgentStreamEvent
        {
            Type = "started",
            Message = $"Started task {taskId}"
        };

        // Execute task and stream progress updates
        // NOTE: Full streaming with Anthropic SDK requires workaround for C#'s
        // limitation on yield return in try-catch blocks. For Phase 3, we provide
        // basic streaming by executing normally and yielding periodic updates.
        // True token-by-token streaming can be added in Phase 4.

        var result = await ExecuteTaskAsync(task, userId, conversationHistory, cancellationToken);

        if (result.Success)
        {
            yield return new AgentStreamEvent
            {
                Type = "completed",
                Message = "Task completed successfully",
                Content = result.Result
            };
        }
        else
        {
            yield return new AgentStreamEvent
            {
                Type = "error",
                Message = "Task failed",
                Output = result.Error
            };
        }
    }

    /// <summary>
    /// Builds tool definitions from registered tools for Anthropic API
    /// </summary>
    private List<CommonTool> BuildToolDefinitions()
    {
        var tools = new List<CommonTool>();

        foreach (var agentTool in _toolRegistry.GetAllTools())
        {
            // Parse the JSON schema to JsonNode for the Function constructor
            var schemaNode = JsonNode.Parse(agentTool.InputSchemaJson);

            tools.Add(new Function(
                agentTool.Name,
                agentTool.Description,
                schemaNode));
        }

        _logger.LogDebug("Built {Count} tool definitions", tools.Count);
        return tools;
    }

    /// <summary>
    /// Executes tools requested by the agent
    /// </summary>
    private async Task<List<(string ToolUseId, string Content)>> ExecuteToolsAsync(
        List<ToolUseContent> toolUses,
        string userId,
        CancellationToken cancellationToken)
    {
        var results = new List<(string ToolUseId, string Content)>();

        foreach (var toolUse in toolUses)
        {
            _logger.LogInformation("Executing tool: {ToolName} (ID: {ToolUseId})",
                toolUse.Name, toolUse.Id);

            try
            {
                var tool = _toolRegistry.GetTool(toolUse.Name);
                if (tool == null)
                {
                    var errorMsg = $"Tool '{toolUse.Name}' not found in registry";
                    _logger.LogError(errorMsg);
                    results.Add((ToolUseId: toolUse.Id, Content: JsonSerializer.Serialize(new { error = errorMsg })));
                    continue;
                }

                // Convert JsonNode Input to JsonElement for tool execution
                var inputJson = toolUse.Input.ToJsonString();
                var inputElement = JsonSerializer.Deserialize<JsonElement>(inputJson);

                // Execute the tool
                var toolResult = await tool.ExecuteAsync(inputElement, userId, cancellationToken);

                // Add result
                var resultContent = toolResult.Success
                    ? toolResult.Content
                    : JsonSerializer.Serialize(new { error = toolResult.Error });

                results.Add((ToolUseId: toolUse.Id, Content: resultContent));

                _logger.LogInformation("Tool {ToolName} executed successfully: {Success}",
                    toolUse.Name, toolResult.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool {ToolName}", toolUse.Name);
                results.Add((ToolUseId: toolUse.Id, Content: JsonSerializer.Serialize(new { error = ex.Message })));
            }
        }

        return results;
    }
}
