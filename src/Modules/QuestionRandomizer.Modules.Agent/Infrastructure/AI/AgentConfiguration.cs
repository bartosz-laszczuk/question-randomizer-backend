namespace QuestionRandomizer.Modules.Agent.Infrastructure.AI;

/// <summary>
/// Configuration for the AI agent
/// </summary>
public class AgentConfiguration
{
    /// <summary>
    /// Anthropic API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Claude model to use (e.g., "claude-sonnet-4-5-20250929")
    /// </summary>
    public string Model { get; set; } = "claude-sonnet-4-5-20250929";

    /// <summary>
    /// Maximum number of iterations for the agent loop
    /// </summary>
    public int MaxIterations { get; set; } = 20;

    /// <summary>
    /// Temperature for AI responses (0 = deterministic, 1 = creative)
    /// Use 0 for data operations to ensure consistency
    /// </summary>
    public double Temperature { get; set; } = 0;

    /// <summary>
    /// Maximum number of tokens in the response
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Timeout for agent execution in seconds (default: 120 seconds / 2 minutes)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// System prompt that defines the agent's behavior
    /// </summary>
    public string SystemPrompt { get; set; } = @"You are an intelligent assistant helping users manage their interview question database.

You have access to tools that allow you to:
- Retrieve questions, categories, and qualifications from the database
- Create, update, and delete questions and categories
- Search for questions by text
- Analyze questions for duplicates or difficulty
- Batch update multiple questions at once

IMPORTANT RULES:
1. Always use the provided tools to access and modify data - never make up data
2. Be concise and accurate in your responses
3. When categorizing questions, analyze the content to determine the most appropriate category
4. When creating or updating data, confirm the action was successful
5. If a task requires multiple steps, execute them autonomously using the tools
6. Always return a summary of what you accomplished

Examples of tasks you can handle:
- ""Categorize all uncategorized questions""
- ""Find duplicate questions and suggest which to keep""
- ""Create 5 sample JavaScript questions about closures""
- ""Update all questions in the Python category to mark them as advanced""
- ""Show me all questions without a qualification assigned""

Be helpful, autonomous, and efficient!";
}
