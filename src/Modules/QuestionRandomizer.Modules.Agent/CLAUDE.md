# Agent Module - Quick Reference

**Module:** QuestionRandomizer.Modules.Agent
**Purpose:** Integrated AI agent with autonomous task execution
**Technology:** C# + Anthropic SDK + Hangfire + Firestore
**Last Updated:** 2025-12-27

---

## 📚 Documentation Index

- **[AGENT-TOOL-DEVELOPMENT.md](../../../docs/AGENT-TOOL-DEVELOPMENT.md)** - Complete guide to creating new tools
- **[AGENT-TOOLS-REFERENCE.md](../../../docs/AGENT-TOOLS-REFERENCE.md)** - All 15 tools documented
- **[AGENT-EXAMPLES.md](../../../docs/AGENT-EXAMPLES.md)** - Real-world usage examples

---

## Overview

The Agent Module provides autonomous AI-powered task execution integrated into the backend API. It replaces the previous TypeScript Agent Service, consolidating from 3 services to 2.

**Architecture Evolution:**
```
BEFORE: Frontend → Backend → TypeScript Agent Service
AFTER:  Frontend → Backend (with integrated Agent Module)
```

**Key Features:**
- 15 specialized tools (6 retrieval, 7 modification, 2 analysis)
- Background processing with Hangfire
- Firestore persistence for task status/results
- Automatic retry with exponential backoff (5s, 15s, 30s)
- Timeout protection (default: 120s)
- Secure user data isolation

**Module Stats:** 35 C# files, 4 key components (AgentExecutor, ToolRegistry, TaskQueueService, AgentTaskRepository)

---

## Quick Start

### Queue a Task

```csharp
// In your controller/handler
var taskId = await _taskQueueService.QueueTaskAsync(
    task: "Categorize all uncategorized questions about JavaScript",
    userId: currentUserId,
    cancellationToken
);
// Returns immediately with taskId
```

### Check Status

```csharp
var taskStatus = await _taskQueueService.GetTaskWithUserIdAsync(
    taskId, userId, cancellationToken);

if (taskStatus?.Status == "completed")
{
    var result = taskStatus.Result;
}
```

### API Endpoints

```
POST /api/agent/queue          # Queue background task
GET  /api/agent/tasks/{id}     # Get task status/result
```

---

## Architecture

```
Agent Module
├── Domain
│   └── AgentTask              # Task entity (status, result, metadata)
│
├── Application
│   ├── Interfaces             # IAgentExecutor, IAgentTaskRepository, ITaskQueueService
│   └── Tools (15 total)
│       ├── DataRetrieval (6)  # Get questions, categories, search, etc.
│       ├── DataModification (7) # Create, update, delete, batch ops
│       └── DataAnalysis (2)   # Find duplicates, analyze difficulty
│
└── Infrastructure
    ├── AI
    │   ├── AgentExecutor      # Claude SDK integration, tool calling loop
    │   └── AgentConfiguration # Model, temperature, timeout settings
    ├── Queue
    │   ├── TaskQueueService   # Hangfire queue management
    │   └── AgentTaskProcessor # Background worker with retry
    └── Repositories
        └── AgentTaskRepository # Firestore persistence
```

**Flow:**
1. User queues task → TaskQueueService
2. Hangfire picks up job → AgentTaskProcessor
3. AgentExecutor runs agent loop with Claude API
4. Tools called autonomously based on task
5. Result/error stored in Firestore
6. User retrieves result via API

---

## Available Tools

**Data Retrieval (6):**
- `get_questions` - List all questions
- `get_question_by_id` - Get single question
- `get_categories` - List categories
- `get_qualifications` - List qualifications
- `get_uncategorized_questions` - Find uncategorized
- `search_questions` - Full-text search

**Data Modification (7):**
- `create_question` - Create new question
- `update_question` - Update question
- `delete_question` - Soft delete question
- `update_question_category` - Update category only
- `create_category` - Create new category
- `create_qualification` - Create new qualification
- `batch_update_questions` - Bulk update (optimized)

**Data Analysis (2):**
- `find_duplicate_questions` - Identify duplicates with similarity scores
- `analyze_question_difficulty` - Suggest difficulty levels

**📖 See [AGENT-TOOLS-REFERENCE.md](../../../docs/AGENT-TOOLS-REFERENCE.md) for complete tool documentation with schemas and examples.**

---

## Configuration

**Quick Reference:**

| Setting | Default | Purpose |
|---------|---------|---------|
| `Model` | `claude-sonnet-4-5-20250929` | Claude model to use |
| `Temperature` | `0` | Response randomness (0 = deterministic) |
| `MaxIterations` | `20` | Max agent loop iterations |
| `MaxTokens` | `4096` | Max response tokens |
| `TimeoutSeconds` | `120` | Execution timeout (2 min) |

**appsettings.json:**
```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-api03-...",
    "Model": "claude-sonnet-4-5-20250929",
    "Temperature": 0,
    "MaxIterations": 20,
    "MaxTokens": 4096,
    "TimeoutSeconds": 120
  }
}
```

**Model Selection:**
- **Sonnet 4.5** (default) - Best balance for production
- **Opus 4.5** - Highest intelligence for complex tasks
- **Haiku 4** - Fastest/cheapest for simple tasks

**Temperature Guide:**
- **0** (recommended) - Deterministic for data operations
- **0.3-0.5** - Slight creativity for analysis
- **0.7-1.0** - Creative (not recommended for data ops)

---

## Background Processing

**Task Lifecycle:**
```
pending → processing → completed/failed
```

**Hangfire Retry:**
- Attempt 1: Immediate
- Attempt 2: After 5 seconds
- Attempt 3: After 15 seconds
- Attempt 4: After 30 seconds
- After 4 failures: Task marked as failed

**Timeout Protection:**
- Wraps execution with configurable timeout
- Prevents hanging tasks
- Returns error if exceeded

**Firestore Collection:** `agent_tasks`
```json
{
  "taskId": "guid",
  "userId": "user-123",
  "taskDescription": "Categorize questions...",
  "status": "completed",
  "result": "Successfully categorized...",
  "createdAt": "2025-12-27T10:00:00Z",
  "completedAt": "2025-12-27T10:00:45Z"
}
```

---

## Creating New Tools

**Quick Workflow:**

1. **Create tool class** in `Application/Tools/[Category]/`
2. **Inherit from `AgentToolBase`**
3. **Define Name, Description, InputSchemaJson**
4. **Implement `ExecuteInternalAsync`**
5. **Register in `AgentModuleExtensions.cs`**
6. **Write unit tests**

**Minimal Example:**
```csharp
public class MyTool : AgentToolBase
{
    public override string Name => "my_tool";
    public override string Description => "Does something useful";
    public override string InputSchemaJson => @"{
        ""type"": ""object"",
        ""properties"": {
            ""param"": { ""type"": ""string"" }
        },
        ""required"": [""param""]
    }";

    protected override async Task<ToolResult> ExecuteInternalAsync(
        JsonElement input, string userId, CancellationToken cancellationToken)
    {
        var param = input.GetProperty("param").GetString();
        // ... do work ...
        return ToolResult.Success(JsonSerializer.Serialize(result));
    }
}
```

**📖 See [AGENT-TOOL-DEVELOPMENT.md](../../../docs/AGENT-TOOL-DEVELOPMENT.md) for complete step-by-step guide.**

---

## Common Tasks

**Categorize Questions:**
```
Task: "Categorize all uncategorized questions"
Flow: get_uncategorized_questions → get_categories → update_question_category (per question)
```

**Find Duplicates:**
```
Task: "Find and delete duplicate questions"
Flow: find_duplicate_questions → delete_question (for redundant ones)
```

**Batch Create:**
```
Task: "Create 5 JavaScript questions about closures"
Flow: get_categories → get_qualifications → create_question (5 times)
```

**Analyze Difficulty:**
```
Task: "Analyze Python question difficulty"
Flow: search_questions → analyze_question_difficulty → summary report
```

**📖 See [AGENT-EXAMPLES.md](../../../docs/AGENT-EXAMPLES.md) for 6 detailed examples with agent decision flows.**

---

## Testing

**Unit Test Pattern:**
```csharp
public class ToolTests
{
    private readonly Mock<IRepository> _mockRepo;
    private readonly MyTool _tool;

    [Fact]
    public async Task ExecuteAsync_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAsync(...)).ReturnsAsync(data);
        var input = JsonDocument.Parse(@"{""id"": ""123""}").RootElement;

        // Act
        var result = await _tool.ExecuteAsync(input, "user-123", CancellationToken.None);

        // Assert
        Assert.True(result.Success);
    }
}
```

**Integration Test:**
```csharp
[Fact]
public async Task Agent_ExecutesTask_Successfully()
{
    var result = await _agentExecutor.ExecuteTaskAsync(
        "Get all categories", "user-123", CancellationToken.None);

    Assert.True(result.Success);
}
```

**📖 See [TESTING.md](../../../docs/TESTING.md) for general testing guide.**

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Tool not found" | Check DI registration in `AgentModuleExtensions.cs` |
| "Task timeout" | Increase `TimeoutSeconds` in configuration |
| "API key required" | Set `Anthropic:ApiKey` in `appsettings.json` |
| "Max iterations" | Increase `MaxIterations` or simplify task |
| Jobs not processing | Verify Hangfire server running, check `WorkerCount` |

**Enable Debug Logging:**
```json
{
  "Logging": {
    "LogLevel": {
      "QuestionRandomizer.Modules.Agent": "Debug"
    }
  }
}
```

---

## Best Practices

**Task Descriptions:**
- ✅ Clear and specific: "Categorize all uncategorized JavaScript questions"
- ❌ Vague: "Fix the questions"

**Tool Development:**
- ✅ Always use `userId` for data filtering
- ✅ Return structured JSON
- ✅ Validate required fields
- ✅ Provide helpful error messages

**Security:**
- ✅ Filter all data by userId
- ✅ Validate all inputs
- ✅ Never trust client data
- ✅ Use soft deletes

**Performance:**
- ✅ Use `batch_update_questions` for bulk operations
- ✅ Limit result sizes
- ✅ Set appropriate timeouts

---

## Related Documentation

**Module-Specific:**
- [AGENT-TOOL-DEVELOPMENT.md](../../../docs/AGENT-TOOL-DEVELOPMENT.md) - Tool development guide
- [AGENT-TOOLS-REFERENCE.md](../../../docs/AGENT-TOOLS-REFERENCE.md) - All 15 tools documented
- [AGENT-EXAMPLES.md](../../../docs/AGENT-EXAMPLES.md) - Real-world examples

**General Backend:**
- [Main CLAUDE.md](../../../CLAUDE.md) - Backend developer guide
- [ARCHITECTURE.md](../../../../ARCHITECTURE.md) - System architecture
- [CODE-TEMPLATES.md](../../../docs/CODE-TEMPLATES.md) - Code patterns
- [TESTING.md](../../../docs/TESTING.md) - Testing strategy

**External:**
- [Anthropic SDK](https://github.com/Anthropic/anthropic-sdk-dotnet)
- [Claude API Docs](https://docs.anthropic.com/)
- [Hangfire Docs](https://www.hangfire.io/)

---

**Last Updated:** 2025-12-27
**Status:** Production Ready (Phase 3 Complete)
**Lines:** ~230 (Optimized from 1,568)
