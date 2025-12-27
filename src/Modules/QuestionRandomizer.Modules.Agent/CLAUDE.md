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
- **🆕 Conversational Context** - Multi-turn conversations with full history
- 15 specialized tools (6 retrieval, 7 modification, 2 analysis)
- Background processing with Hangfire
- Firestore persistence for task status/results & conversations
- Automatic retry with exponential backoff (5s, 15s, 30s)
- Timeout protection (default: 120s)
- Secure user data isolation

**Module Stats:** 35 C# files, 4 key components (AgentExecutor, ToolRegistry, TaskQueueService, AgentTaskRepository)

---

## Quick Start

### One-Shot Task (No Conversation)

```csharp
// In your controller/handler
var taskId = await _taskQueueService.QueueTaskAsync(
    task: "Categorize all uncategorized questions about JavaScript",
    userId: currentUserId,
    conversationId: null,  // Creates new conversation
    cancellationToken
);
// Returns immediately with taskId
```

### 🆕 Conversational Task (With Context)

```csharp
// Turn 1: Start conversation
var taskId1 = await _taskQueueService.QueueTaskAsync(
    task: "Update all uncategorized questions",
    userId: currentUserId,
    conversationId: null,  // Creates conversation "conv-123"
    cancellationToken
);

// Turn 2: Continue conversation (agent remembers Turn 1!)
var taskId2 = await _taskQueueService.QueueTaskAsync(
    task: "Provide me the ids of all updated questions",
    userId: currentUserId,
    conversationId: "conv-123",  // Uses conversation history
    cancellationToken
);
// Agent will know which questions were updated in Turn 1!
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
POST /api/agent/queue                # Queue background task
POST /api/agent/execute              # Execute synchronously
POST /api/agent/execute/stream       # Execute with streaming (sync)
GET  /api/agent/tasks/{id}           # Get task status/result
GET  /api/agent/tasks/{id}/stream    # Stream real-time updates for queued task
```

**API Request Examples:**

```json
// New conversation
POST /api/agent/queue
{
  "task": "Update all uncategorized questions"
}

// Continue conversation
POST /api/agent/queue
{
  "task": "Provide the ids",
  "conversationId": "conv-123"
}
```

**🆕 Real-time Streaming (Recommended for Chat UIs):**

```typescript
// Queue task
const { taskId } = await fetch('/api/agent/queue', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ task: 'Update all uncategorized questions' })
}).then(r => r.json());

// Subscribe to real-time updates via SSE
const eventSource = new EventSource(`/api/agent/tasks/${taskId}/stream`);

eventSource.addEventListener('started', (e) => {
  console.log('Task queued...');
});

eventSource.addEventListener('status_change', (e) => {
  const data = JSON.parse(e.data);
  console.log('Status:', data.Output); // "processing", "completed", etc.
});

eventSource.addEventListener('completed', (e) => {
  const data = JSON.parse(e.data);
  console.log('Result:', data.Content);
  eventSource.close();
});

eventSource.addEventListener('error', (e) => {
  const data = JSON.parse(e.data);
  console.error('Error:', data.Message);
  eventSource.close();
});
```

**Stream Event Types:**
- `started` - Stream initialized, task queued
- `status_change` - Status updated (queued → processing → completed/failed)
- `completed` - Task finished successfully (includes result)
- `error` - Task failed or not found (includes error message)

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
3. **🆕 Load conversation history** (if conversationId provided)
4. **🆕 Save user message** to conversation
5. AgentExecutor runs agent loop with Claude API + conversation context
6. Tools called autonomously based on task
7. **🆕 Save agent response** to conversation
8. Result/error stored in Firestore
9. User retrieves result via API

---

## 🆕 Conversational Context

The Agent Module now supports **multi-turn conversations** with full context retention, enabling ChatGPT-like interactions.

### How It Works

**Without conversationId (One-Shot):**
- Creates a new conversation automatically
- Agent has NO previous context
- Use for standalone tasks

**With conversationId (Conversational):**
- Fetches conversation history from Firestore
- Agent sees ALL previous messages (user + assistant)
- Use for follow-up questions, refinements, multi-step tasks

### Example Flow

```
Turn 1: "Update all uncategorized questions"
├─ Creates conversation: conv-123
├─ User message saved: "Update all uncategorized questions"
├─ Agent executes with NO history
├─ Agent response: "✅ Updated 15 questions: q-1, q-2, q-3..."
└─ Response saved to conversation

Turn 2: "Provide me the ids" (conversationId: conv-123)
├─ Loads history: Turn 1 messages
├─ User message saved: "Provide me the ids"
├─ Agent sees FULL CONTEXT (knows about the 15 updated questions!)
├─ Agent response: "The updated IDs are: q-1, q-2, q-3, q-4..."
└─ Response saved to conversation

Turn 3: "Delete the first 3" (conversationId: conv-123)
├─ Loads history: Turn 1 + Turn 2 messages
├─ Agent knows which questions AND which IDs!
├─ Agent response: "✅ Deleted q-1, q-2, q-3"
└─ Continues naturally like ChatGPT
```

### Integration with Conversations Module

The Agent Module integrates seamlessly with the **Conversations Module**:
- **Creates conversations** automatically when conversationId is null
- **Loads messages** using `IMessageRepository.GetByConversationIdAsync()`
- **Saves messages** using `IMessageRepository.CreateAsync()`
- **Updates timestamps** using `IConversationRepository.UpdateTimestampAsync()`
- **Secure by design** - All operations filtered by userId

### Conversation Storage

**Firestore Collections:**
```
conversations/               # Conversation metadata
  conv-123/
    - id: "conv-123"
    - userId: "user-456"
    - title: "Update all uncategorized..."
    - createdAt: "2025-12-27T10:00:00Z"
    - updatedAt: "2025-12-27T10:05:00Z"

messages/                   # Conversation messages
  msg-1/
    - conversationId: "conv-123"
    - role: "user"
    - content: "Update all uncategorized questions"
    - timestamp: "2025-12-27T10:00:00Z"
  msg-2/
    - conversationId: "conv-123"
    - role: "assistant"
    - content: "✅ Updated 15 questions..."
    - timestamp: "2025-12-27T10:00:45Z"
```

### Use Cases

**✅ Perfect for Conversational Tasks:**
- Multi-step workflows: "Update X", then "Provide details", then "Delete some"
- Iterative refinement: "Find duplicates", then "Delete the ones from category X"
- Follow-up questions: "Categorize questions", then "How many did you categorize?"
- Context-dependent operations: "Show me Python questions", then "Update them all"

**❌ Not Needed for One-Shot Tasks:**
- Single operations: "Categorize all uncategorized questions" (complete in one go)
- Independent tasks: Each task is unrelated to previous ones

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
