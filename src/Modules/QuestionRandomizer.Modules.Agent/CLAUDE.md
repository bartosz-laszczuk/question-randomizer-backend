# Agent Module - Quick Reference

**Module:** QuestionRandomizer.Modules.Agent
**Purpose:** Integrated AI agent with autonomous task execution
**Technology:** C# + Anthropic SDK + Firestore
**Last Updated:** 2025-12-30

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
- **Conversational Context** - Multi-turn conversations with full history
- **Real-time Streaming** - ChatGPT-like streaming execution (SSE)
- 15 specialized tools (6 retrieval, 7 modification, 2 analysis)
- Firestore persistence for conversations & messages
- Timeout protection (default: 300s / 5 minutes)
- Secure user data isolation

**Module Stats:** Clean & minimal - AgentExecutor, ToolRegistry, AgentService, 15 tools

---

## Quick Start

### One-Shot Task (No Conversation)

```csharp
// In your controller/handler - Stream execution events
await foreach (var streamEvent in _agentService.ExecuteTaskStreamingAsync(
    task: "Categorize all uncategorized questions about JavaScript",
    userId: currentUserId,
    conversationId: null,  // Creates new conversation
    cancellationToken))
{
    // Handle real-time events
    switch (streamEvent.Type)
    {
        case "started":
            Console.WriteLine("Task started...");
            break;
        case "thinking":
            Console.WriteLine("Agent is thinking...");
            break;
        case "text_chunk":
            Console.Write(streamEvent.Content);  // Stream response text
            break;
        case "tool_call":
            Console.WriteLine($"Calling tool: {streamEvent.ToolName}");
            break;
        case "completed":
            Console.WriteLine($"\nCompleted: {streamEvent.Content}");
            break;
        case "error":
            Console.WriteLine($"Error: {streamEvent.Message}");
            break;
    }
}
```

### 🆕 Conversational Task (With Context)

```csharp
// Turn 1: Start conversation
await foreach (var event in _agentService.ExecuteTaskStreamingAsync(
    task: "Update all uncategorized questions",
    userId: currentUserId,
    conversationId: null,  // Creates conversation "conv-123"
    cancellationToken))
{
    HandleStreamEvent(event);
}

// Turn 2: Continue conversation (agent remembers Turn 1!)
await foreach (var event in _agentService.ExecuteTaskStreamingAsync(
    task: "Provide me the ids of all updated questions",
    userId: currentUserId,
    conversationId: "conv-123",  // Uses conversation history
    cancellationToken))
{
    HandleStreamEvent(event);
}
// Agent will know which questions were updated in Turn 1!
```

### API Endpoint

**HTTP Streaming Endpoint:**
```
POST /api/agent/execute     # Execute task with real-time streaming (SSE)
```

**Request:**
```json
{
  "task": "Update all uncategorized questions",
  "conversationId": null  // Optional - for multi-turn conversations
}
```

**Response:** Server-Sent Events (SSE) stream with AgentStreamEvent objects

**Stream Event Types:**
- `started` - Task execution started
- `thinking` - Agent is analyzing/planning
- `text_chunk` - Streaming response text (ChatGPT-like)
- `tool_call` - Agent calling a tool
- `tool_result` - Tool execution completed
- `completed` - Task finished successfully
- `error` - Task failed with error

### Frontend Example (Fetch API + SSE)

```typescript
// Execute task with streaming
const response = await fetch('http://localhost:5000/api/agent/execute', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${firebaseToken}`
  },
  body: JSON.stringify({
    task: 'Categorize all uncategorized questions',
    conversationId: null  // or "conv-123" for continuation
  })
});

// Read SSE stream
const reader = response.body.getReader();
const decoder = new TextDecoder();
let buffer = '';

while (true) {
  const { done, value } = await reader.read();
  if (done) break;

  buffer += decoder.decode(value, { stream: true });
  const lines = buffer.split('\n');
  buffer = lines.pop() || '';

  for (const line of lines) {
    if (line.startsWith('data: ')) {
      const event = JSON.parse(line.slice(6));

      switch (event.type) {
        case 'started':
          console.log('Started...');
          break;
        case 'text_chunk':
          process.stdout.write(event.content);  // Stream like ChatGPT
          break;
        case 'tool_call':
          console.log(`\nCalling: ${event.toolName}`);
          break;
        case 'completed':
          console.log('\nCompleted!');
          break;
        case 'error':
          console.error('Error:', event.message);
          break;
      }
    }
  }
}
```

### Conversational Context Example

```typescript
// Turn 1: Start new conversation
const response1 = await executeTask({
  task: 'Update all uncategorized questions'
});
// Response includes conversationId: "conv-123"

// Turn 2: Continue conversation (agent remembers context!)
const response2 = await executeTask({
  task: 'Provide me the ids',
  conversationId: 'conv-123'
});
// Agent knows which questions were updated in Turn 1

// Turn 3: Further refinement
const response3 = await executeTask({
  task: 'Delete the first 3',
  conversationId: 'conv-123'
});
// Agent knows the exact IDs from Turn 2
```

**Benefits:**
- ✅ **ChatGPT-like streaming** - Real-time text chunks
- ✅ **5-minute timeout** - Long-running tasks supported
- ✅ **Conversational context** - Multi-turn conversations
- ✅ **Simple HTTP** - No WebSocket complexity
- ✅ **Automatic reconnection** - Client-side retry logic

---

## Architecture

```
Agent Module
├── Application
│   ├── DTOs
│   │   ├── ExecuteTaskRequest   # API request model
│   │   └── AgentStreamEvent     # Streaming event model
│   ├── Interfaces
│   │   ├── IAgentExecutor       # Core agent execution interface
│   │   └── IAgentService        # High-level service interface
│   └── Tools (15 total)
│       ├── DataRetrieval (6)    # Get questions, categories, search, etc.
│       ├── DataModification (7) # Create, update, delete, batch ops
│       └── DataAnalysis (2)     # Find duplicates, analyze difficulty
│
└── Infrastructure
    ├── AI
    │   ├── AgentExecutor        # Claude SDK integration, streaming loop
    │   └── AgentConfiguration   # Model, temperature, timeout settings
    └── Services
        └── AgentService         # Conversation context + streaming orchestration
```

**Streaming Flow:**
1. Client sends POST /api/agent/execute
2. AgentService loads conversation history (if conversationId provided)
3. AgentService saves user message to conversation
4. AgentExecutor runs streaming agent loop:
   - Streams "started" event
   - Streams "thinking" events during analysis
   - Streams "text_chunk" events for response text (ChatGPT-like)
   - Streams "tool_call" + "tool_result" events when using tools
   - Tools called autonomously based on task
   - Streams "completed" event with final result
5. AgentService saves agent response to conversation
6. Client receives real-time Server-Sent Events (SSE)

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
| `TimeoutSeconds` | `300` | Execution timeout (5 min) |

**appsettings.json:**
```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-api03-...",
    "Model": "claude-sonnet-4-5-20250929",
    "Temperature": 0,
    "MaxIterations": 20,
    "MaxTokens": 4096,
    "TimeoutSeconds": 300
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
| "Task timeout" | Increase `TimeoutSeconds` in configuration (default: 300s) |
| "API key required" | Set `Anthropic:ApiKey` in `appsettings.json` |
| "Max iterations" | Increase `MaxIterations` or simplify task |
| Stream disconnects | Check Kestrel timeout settings (KeepAliveTimeout, RequestHeadersTimeout) |

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

---

**Last Updated:** 2025-12-30
**Status:** Production Ready - Clean streaming implementation
