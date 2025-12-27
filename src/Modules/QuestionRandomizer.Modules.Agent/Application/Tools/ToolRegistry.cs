namespace QuestionRandomizer.Modules.Agent.Application.Tools;

using QuestionRandomizer.Modules.Agent.Application.Tools.Base;

/// <summary>
/// Central registry for all agent tools
/// Manages tool discovery and lookup
/// </summary>
public class ToolRegistry
{
    private readonly Dictionary<string, IAgentTool> _tools;

    public ToolRegistry(IEnumerable<IAgentTool> tools)
    {
        _tools = tools.ToDictionary(t => t.Name, t => t);
    }

    /// <summary>
    /// Gets a tool by name
    /// </summary>
    public IAgentTool? GetTool(string name)
    {
        _tools.TryGetValue(name, out var tool);
        return tool;
    }

    /// <summary>
    /// Gets all registered tools
    /// </summary>
    public IEnumerable<IAgentTool> GetAllTools()
    {
        return _tools.Values;
    }

    /// <summary>
    /// Gets the count of registered tools
    /// </summary>
    public int Count => _tools.Count;

    /// <summary>
    /// Checks if a tool with the given name exists
    /// </summary>
    public bool HasTool(string name)
    {
        return _tools.ContainsKey(name);
    }
}
