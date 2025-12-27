# Agent Module - Developer Guide

**Module:** QuestionRandomizer.Modules.Agent
**Purpose:** Integrated AI agent with autonomous task execution
**Technology:** C# + Anthropic SDK + Hangfire + Firestore
**Last Updated:** 2025-12-27

---

## 📚 Table of Contents

1. [Overview](#overview)
2. [Quick Start](#quick-start)
3. [Architecture](#architecture)
4. [Tool Development Guide](#tool-development-guide)
5. [Available Tools Reference](#available-tools-reference)
6. [Configuration](#configuration)
7. [Background Processing](#background-processing)
8. [Testing](#testing)
9. [Troubleshooting](#troubleshooting)
10. [Examples](#examples)

---

## Overview

### What is the Agent Module?

The Agent Module provides autonomous AI-powered task execution integrated directly into the backend API. It replaces the previous TypeScript Agent Service, consolidating the architecture from 3 services to 2.

**Key Capabilities:**
- Execute complex tasks autonomously using Claude AI
- 15 specialized tools for data operations (retrieval, modification, analysis)
- Background processing with Hangfire
- Firestore persistence for task status and results
- Automatic retry with exponential backoff
- Timeout protection
- Secure user data isolation

**Architecture Shift:**
```
BEFORE (3-Service):                    AFTER (2-Service):
Frontend → Backend → Agent Service     Frontend → Backend (with Agent Module)
```

### Module Statistics

- **Total Files:** 35 C# files
- **Tools:** 15 specialized tools
  - 6 Data Retrieval tools
  - 7 Data Modification tools
  - 2 Data Analysis tools
- **Key Components:**
  - AgentExecutor - Main execution engine
  - ToolRegistry - Tool discovery and registration
  - TaskQueueService - Background queue management
  - AgentTaskRepository - Firestore persistence

---

## Quick Start

### Basic Usage

**1. Queue an Agent Task**

```csharp
// Inject ITaskQueueService in your controller/handler
private readonly ITaskQueueService _taskQueueService;

// Queue a task for background processing
var taskId = await _taskQueueService.QueueTaskAsync(
    task: "Categorize all uncategorized questions about JavaScript",
    userId: currentUserId,
    cancellationToken
);

// Returns immediately with taskId for status tracking
```

**2. Check Task Status**

```csharp
// Get task status and result
var taskStatus = await _taskQueueService.GetTaskWithUserIdAsync(
    taskId,
    userId,
    cancellationToken
);

// Status: "pending", "processing", "completed", "failed"
if (taskStatus?.Status == "completed")
{
    var result = taskStatus.Result; // Agent's response
}
```

**3. API Endpoints**

```http
POST /api/agent/queue
{
  "task": "Find duplicate questions and suggest which to keep"
}

GET /api/agent/tasks/{taskId}
```

---

## Architecture

### Component Overview

```
Agent Module
├── Domain
│   └── AgentTask.cs                    // Task entity (status, result, metadata)
│
├── Application
│   ├── Interfaces
│   │   ├── IAgentExecutor.cs          // Main execution interface
│   │   ├── IAgentTaskRepository.cs    // Firestore repository
│   │   └── ITaskQueueService.cs       // Queue service
│   │
│   └── Tools
│       ├── Base
│       │   ├── IAgentTool.cs          // Tool interface
│       │   ├── AgentToolBase.cs       // Base implementation
│       │   └── ToolResult.cs          // Result wrapper
│       │
│       ├── DataRetrieval (6 tools)
│       ├── DataModification (7 tools)
│       └── DataAnalysis (2 tools)
│
└── Infrastructure
    ├── AI
    │   ├── AgentExecutor.cs           // Claude SDK integration
    │   └── AgentConfiguration.cs      // Configuration settings
    │
    ├── Queue
    │   ├── TaskQueueService.cs        // Queue management
    │   └── AgentTaskProcessor.cs      // Hangfire worker
    │
    └── Repositories
        └── AgentTaskRepository.cs     // Firestore persistence
```

### Key Components Explained

#### 1. AgentExecutor

**Purpose:** Main execution engine that orchestrates the agent loop with Claude API

**Flow:**
```
1. Receives task description
2. Builds tool definitions from ToolRegistry
3. Calls Claude API with tools available
4. Claude decides which tools to use
5. Executes tools and returns results to Claude
6. Repeats until task complete or max iterations
7. Returns final response
```

**Key Features:**
- Autonomous tool calling loop
- Timeout protection (configurable, default: 120s)
- Comprehensive error handling
- Logging at each step

#### 2. ToolRegistry

**Purpose:** Dynamic tool discovery and registration

**How it works:**
```csharp
// All IAgentTool implementations are automatically discovered via DI
public class ToolRegistry
{
    private readonly IEnumerable<IAgentTool> _tools;

    public IAgentTool? GetTool(string name)
    {
        return _tools.FirstOrDefault(t => t.Name == name);
    }

    public List<IAgentTool> GetAllTools() => _tools.ToList();
}
```

#### 3. Hangfire Background Queue

**Purpose:** Async task processing with automatic retry

**Features:**
- Background job processing
- Automatic retry: 3 attempts with exponential backoff (5s, 15s, 30s)
- Job cleanup (configurable retention)
- Concurrent worker support

#### 4. Firestore Persistence

**Collection:** `agent_tasks`

**Document Structure:**
```json
{
  "taskId": "guid",
  "userId": "user-123",
  "taskDescription": "Categorize questions...",
  "status": "completed",
  "result": "Successfully categorized 15 questions...",
  "error": null,
  "jobId": "hangfire-job-id",
  "createdAt": "2025-12-27T10:00:00Z",
  "startedAt": "2025-12-27T10:00:01Z",
  "completedAt": "2025-12-27T10:00:45Z",
  "metadata": {}
}
```

---

## Tool Development Guide

### Creating a New Tool

Follow these steps to add a new agent tool:

#### Step 1: Create Tool Class

Create a new file in the appropriate folder:
- `Application/Tools/DataRetrieval/` - For reading data
- `Application/Tools/DataModification/` - For writing data
- `Application/Tools/DataAnalysis/` - For analysis operations

```csharp
using System.Text.Json;
using QuestionRandomizer.Modules.Agent.Application.Tools.Base;

namespace QuestionRandomizer.Modules.Agent.Application.Tools.DataRetrieval;

/// <summary>
/// Retrieves questions by qualification ID
/// </summary>
public class GetQuestionsByQualificationTool : AgentToolBase
{
    private readonly IQuestionRepository _questionRepository;

    public GetQuestionsByQualificationTool(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    // Step 2: Define tool metadata
    public override string Name => "get_questions_by_qualification";

    public override string Description =>
        "Retrieves all questions associated with a specific qualification ID";

    // Step 3: Define input schema
    public override string InputSchemaJson => @"{
        ""type"": ""object"",
        ""properties"": {
            ""qualificationId"": {
                ""type"": ""string"",
                ""description"": ""The ID of the qualification""
            }
        },
        ""required"": [""qualificationId""]
    }";

    // Step 4: Implement execution logic
    protected override async Task<ToolResult> ExecuteInternalAsync(
        JsonElement input,
        string userId,
        CancellationToken cancellationToken)
    {
        // Parse input
        var qualificationId = input.GetProperty("qualificationId").GetString();

        if (string.IsNullOrEmpty(qualificationId))
        {
            return ToolResult.Failure("qualificationId is required");
        }

        // Execute business logic
        var questions = await _questionRepository.GetByQualificationAsync(
            qualificationId,
            userId,
            cancellationToken);

        // Return result as JSON
        var result = new
        {
            count = questions.Count,
            questions = questions.Select(q => new
            {
                id = q.Id,
                text = q.QuestionText,
                categoryId = q.CategoryId
            })
        };

        return ToolResult.Success(JsonSerializer.Serialize(result));
    }
}
```

#### Step 5: Register Tool in DI

Add to `AgentModuleExtensions.cs`:

```csharp
// Data Retrieval Tools
services.AddScoped<IAgentTool, GetQuestionsTool>();
services.AddScoped<IAgentTool, GetQuestionByIdTool>();
services.AddScoped<IAgentTool, GetQuestionsByQualificationTool>(); // NEW TOOL
```

#### Step 6: Test Your Tool

Create a unit test in `tests/QuestionRandomizer.Modules.Agent.Tests/`:

```csharp
public class GetQuestionsByQualificationToolTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidQualificationId_ReturnsQuestions()
    {
        // Arrange
        var mockRepo = new Mock<IQuestionRepository>();
        mockRepo.Setup(r => r.GetByQualificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question> { /* test data */ });

        var tool = new GetQuestionsByQualificationTool(mockRepo.Object);
        var input = JsonDocument.Parse(@"{""qualificationId"": ""qual-123""}").RootElement;

        // Act
        var result = await tool.ExecuteAsync(input, "user-123", CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("questions", result.Content);
    }
}
```

### Tool Development Best Practices

**1. Input Validation**
- Always validate required parameters
- Return clear error messages for invalid input
- Use JSON schema to define expected structure

**2. Security**
- ALWAYS use the provided `userId` parameter to filter data
- Never trust input data blindly
- Validate IDs before database queries

**3. Error Handling**
- Catch expected exceptions and return `ToolResult.Failure()`
- Let unexpected exceptions bubble up for logging
- Provide helpful error messages to the agent

**4. Performance**
- Use async/await throughout
- Respect cancellation tokens
- Limit result sizes (pagination if needed)

**5. Output Format**
- Return structured JSON for complex data
- Keep output concise and relevant
- Include counts/metadata when appropriate

---

## Available Tools Reference

### Data Retrieval Tools (6)

#### 1. GetQuestionsTool

**Name:** `get_questions`
**Purpose:** Retrieve all questions for the authenticated user

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "limit": {
      "type": "number",
      "description": "Maximum number of questions to return (default: 100)"
    }
  }
}
```

**Output:**
```json
{
  "count": 42,
  "questions": [
    {
      "id": "q-123",
      "questionText": "What is a closure?",
      "categoryId": "cat-456",
      "qualificationId": "qual-789",
      "isDeleted": false
    }
  ]
}
```

**Example Usage:**
```
Task: "Show me all my questions about JavaScript"
Agent will call: get_questions with appropriate filters
```

---

#### 2. GetQuestionByIdTool

**Name:** `get_question_by_id`
**Purpose:** Retrieve a single question by its ID

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "questionId": {
      "type": "string",
      "description": "The ID of the question to retrieve"
    }
  },
  "required": ["questionId"]
}
```

**Output:**
```json
{
  "id": "q-123",
  "questionText": "What is a closure?",
  "categoryId": "cat-456",
  "qualificationId": "qual-789",
  "isDeleted": false,
  "createdAt": "2025-01-15T10:00:00Z"
}
```

---

#### 3. GetCategoriesTool

**Name:** `get_categories`
**Purpose:** Retrieve all categories

**Input Schema:**
```json
{
  "type": "object",
  "properties": {}
}
```

**Output:**
```json
{
  "count": 5,
  "categories": [
    {
      "id": "cat-123",
      "name": "JavaScript",
      "description": "JavaScript programming questions"
    }
  ]
}
```

---

#### 4. GetQualificationsTool

**Name:** `get_qualifications`
**Purpose:** Retrieve all qualifications

**Input Schema:**
```json
{
  "type": "object",
  "properties": {}
}
```

**Output:**
```json
{
  "count": 3,
  "qualifications": [
    {
      "id": "qual-123",
      "name": "Senior Frontend Developer",
      "description": "Senior level frontend position"
    }
  ]
}
```

---

#### 5. GetUncategorizedQuestionsTool

**Name:** `get_uncategorized_questions`
**Purpose:** Find questions without a category assigned

**Input Schema:**
```json
{
  "type": "object",
  "properties": {}
}
```

**Output:**
```json
{
  "count": 12,
  "questions": [
    {
      "id": "q-999",
      "questionText": "Explain async/await",
      "categoryId": null,
      "qualificationId": "qual-123"
    }
  ]
}
```

**Common Use Case:**
```
Task: "Categorize all uncategorized questions"
Agent will:
1. Call get_uncategorized_questions
2. Analyze each question
3. Call update_question_category for each
```

---

#### 6. SearchQuestionsTool

**Name:** `search_questions`
**Purpose:** Full-text search across questions

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "searchText": {
      "type": "string",
      "description": "Text to search for in question content"
    },
    "limit": {
      "type": "number",
      "description": "Maximum results to return (default: 50)"
    }
  },
  "required": ["searchText"]
}
```

**Output:**
```json
{
  "count": 8,
  "searchText": "closure",
  "questions": [
    {
      "id": "q-123",
      "questionText": "What is a closure in JavaScript?",
      "categoryId": "cat-456"
    }
  ]
}
```

---

### Data Modification Tools (7)

#### 7. CreateQuestionTool

**Name:** `create_question`
**Purpose:** Create a new question

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "questionText": {
      "type": "string",
      "description": "The question text"
    },
    "categoryId": {
      "type": "string",
      "description": "Category ID (optional)"
    },
    "qualificationId": {
      "type": "string",
      "description": "Qualification ID (optional)"
    }
  },
  "required": ["questionText"]
}
```

**Output:**
```json
{
  "id": "q-new-123",
  "questionText": "What is hoisting?",
  "categoryId": "cat-456",
  "message": "Question created successfully"
}
```

---

#### 8. UpdateQuestionTool

**Name:** `update_question`
**Purpose:** Update an existing question

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "questionId": {
      "type": "string",
      "description": "ID of the question to update"
    },
    "questionText": {
      "type": "string",
      "description": "New question text"
    },
    "categoryId": {
      "type": "string",
      "description": "New category ID"
    },
    "qualificationId": {
      "type": "string",
      "description": "New qualification ID"
    }
  },
  "required": ["questionId"]
}
```

**Output:**
```json
{
  "id": "q-123",
  "message": "Question updated successfully"
}
```

---

#### 9. DeleteQuestionTool

**Name:** `delete_question`
**Purpose:** Soft delete a question

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "questionId": {
      "type": "string",
      "description": "ID of the question to delete"
    }
  },
  "required": ["questionId"]
}
```

**Output:**
```json
{
  "id": "q-123",
  "message": "Question deleted successfully"
}
```

---

#### 10. UpdateQuestionCategoryTool

**Name:** `update_question_category`
**Purpose:** Update only the category of a question

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "questionId": {
      "type": "string",
      "description": "ID of the question"
    },
    "categoryId": {
      "type": "string",
      "description": "New category ID"
    }
  },
  "required": ["questionId", "categoryId"]
}
```

**Output:**
```json
{
  "id": "q-123",
  "categoryId": "cat-456",
  "message": "Question category updated successfully"
}
```

**Optimized for:** Batch categorization tasks

---

#### 11. CreateCategoryTool

**Name:** `create_category`
**Purpose:** Create a new category

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "name": {
      "type": "string",
      "description": "Category name"
    },
    "description": {
      "type": "string",
      "description": "Category description (optional)"
    }
  },
  "required": ["name"]
}
```

**Output:**
```json
{
  "id": "cat-new-123",
  "name": "TypeScript",
  "message": "Category created successfully"
}
```

---

#### 12. CreateQualificationTool

**Name:** `create_qualification`
**Purpose:** Create a new qualification

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "name": {
      "type": "string",
      "description": "Qualification name"
    },
    "description": {
      "type": "string",
      "description": "Qualification description (optional)"
    }
  },
  "required": ["name"]
}
```

**Output:**
```json
{
  "id": "qual-new-123",
  "name": "Lead Developer",
  "message": "Qualification created successfully"
}
```

---

#### 13. BatchUpdateQuestionsTool

**Name:** `batch_update_questions`
**Purpose:** Update multiple questions in one operation

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "updates": {
      "type": "array",
      "description": "Array of question updates",
      "items": {
        "type": "object",
        "properties": {
          "questionId": { "type": "string" },
          "categoryId": { "type": "string" },
          "qualificationId": { "type": "string" }
        },
        "required": ["questionId"]
      }
    }
  },
  "required": ["updates"]
}
```

**Output:**
```json
{
  "totalUpdates": 10,
  "successful": 9,
  "failed": 1,
  "results": [
    {
      "questionId": "q-123",
      "success": true
    },
    {
      "questionId": "q-456",
      "success": false,
      "error": "Question not found"
    }
  ]
}
```

**Performance:** Optimized for bulk operations

---

### Data Analysis Tools (2)

#### 14. FindDuplicateQuestionsTool

**Name:** `find_duplicate_questions`
**Purpose:** Identify potential duplicate questions using similarity analysis

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "similarityThreshold": {
      "type": "number",
      "description": "Similarity threshold 0-1 (default: 0.8)"
    }
  }
}
```

**Output:**
```json
{
  "duplicateGroups": [
    {
      "similarity": 0.95,
      "questions": [
        {
          "id": "q-123",
          "questionText": "What is a closure?"
        },
        {
          "id": "q-456",
          "questionText": "Explain closures in JavaScript"
        }
      ],
      "suggestion": "Keep q-123, delete q-456 (more concise)"
    }
  ],
  "totalDuplicates": 12
}
```

**Use Case:**
```
Task: "Find duplicate questions and suggest which to keep"
Agent analyzes duplicates and provides recommendations
```

---

#### 15. AnalyzeQuestionDifficultyTool

**Name:** `analyze_question_difficulty`
**Purpose:** Analyze and suggest difficulty levels for questions

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "categoryId": {
      "type": "string",
      "description": "Analyze questions in specific category (optional)"
    }
  }
}
```

**Output:**
```json
{
  "analysis": [
    {
      "questionId": "q-123",
      "questionText": "What is a closure?",
      "suggestedDifficulty": "intermediate",
      "reasoning": "Requires understanding of scope and functions",
      "confidence": 0.85
    }
  ],
  "summary": {
    "easy": 15,
    "intermediate": 42,
    "hard": 8
  }
}
```

---

## Configuration

### AgentConfiguration Properties

Located in `Infrastructure/AI/AgentConfiguration.cs`:

```csharp
public class AgentConfiguration
{
    /// <summary>
    /// Anthropic API key (required)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Claude model to use
    /// Default: "claude-sonnet-4-5-20250929"
    /// Options: claude-opus-4-5, claude-sonnet-4-5, claude-haiku-4
    /// </summary>
    public string Model { get; set; } = "claude-sonnet-4-5-20250929";

    /// <summary>
    /// Maximum iterations for agent loop
    /// Default: 20
    /// Prevents infinite loops
    /// </summary>
    public int MaxIterations { get; set; } = 20;

    /// <summary>
    /// Temperature for AI responses (0-1)
    /// 0 = deterministic (recommended for data operations)
    /// 1 = creative
    /// Default: 0
    /// </summary>
    public double Temperature { get; set; } = 0;

    /// <summary>
    /// Maximum tokens in response
    /// Default: 4096
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Timeout for agent execution in seconds
    /// Default: 120 (2 minutes)
    /// Prevents hanging tasks
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// System prompt defining agent behavior
    /// Customizable per environment
    /// </summary>
    public string SystemPrompt { get; set; } = /* default prompt */;
}
```

### appsettings.json Configuration

**Development (`appsettings.Development.json`):**
```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-api03-...",
    "Model": "claude-sonnet-4-5-20250929",
    "MaxIterations": 20,
    "Temperature": 0,
    "MaxTokens": 4096,
    "TimeoutSeconds": 120
  },
  "Hangfire": {
    "WorkerCount": 2
  }
}
```

**Production (`appsettings.Production.json`):**
```json
{
  "Anthropic": {
    "ApiKey": "${ANTHROPIC_API_KEY}",  // From environment variable
    "Model": "claude-sonnet-4-5-20250929",
    "MaxIterations": 30,
    "Temperature": 0,
    "MaxTokens": 8192,
    "TimeoutSeconds": 300
  },
  "Hangfire": {
    "WorkerCount": 5
  }
}
```

### Model Selection Guide

**Claude Sonnet 4.5** (Default - Recommended)
- Best balance of speed and intelligence
- Excellent for data operations
- Cost-effective
- **Use for:** Production workloads

**Claude Opus 4.5**
- Highest intelligence
- Slower, more expensive
- Best for complex reasoning
- **Use for:** Complex analysis tasks, difficult categorization

**Claude Haiku 4**
- Fastest, cheapest
- Good for simple tasks
- **Use for:** Simple data retrieval, basic categorization

### Temperature Tuning

**0.0** (Default - Recommended for data operations)
- Deterministic output
- Consistent categorization
- Predictable tool usage

**0.3-0.5**
- Slight creativity
- Good for analysis tasks

**0.7-1.0**
- Creative responses
- NOT recommended for data operations

---

## Background Processing

### Task Lifecycle

```
1. PENDING
   ↓ (Hangfire picks up job)
2. PROCESSING
   ↓ (Agent executes, tools called)
3a. COMPLETED (success)
   OR
3b. FAILED (error or timeout)
```

### Hangfire Configuration

**Worker Count:**
```csharp
services.AddHangfireServer(options =>
{
    options.WorkerCount = 2; // Concurrent workers
});
```

**Retry Policy:**
```csharp
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 5, 15, 30 })]
public async Task ProcessTaskAsync(...)
```

- **Attempt 1:** Immediate
- **Attempt 2:** After 5 seconds
- **Attempt 3:** After 15 seconds
- **Attempt 4:** After 30 seconds
- **After 4 failures:** Job marked as failed

### Timeout Protection

**How it works:**
```csharp
// AgentExecutor wraps execution with timeout
var executionTask = ExecuteAgentLoopAsync(...);
var timeoutTask = Task.Delay(TimeSpan.FromSeconds(TimeoutSeconds));

var completedTask = await Task.WhenAny(executionTask, timeoutTask);

if (completedTask == timeoutTask)
{
    // Timeout exceeded - return error
    return new AgentTaskResult
    {
        Success = false,
        Error = $"Task exceeded timeout of {TimeoutSeconds} seconds"
    };
}
```

**When to increase timeout:**
- Complex tasks with many tool calls
- Large batch operations
- Slow Firestore queries

**Recommended timeouts:**
- Simple tasks: 60-120 seconds
- Batch operations: 180-300 seconds
- Complex analysis: 300-600 seconds

---

## Testing

### Unit Testing Tools

**Test Structure:**
```csharp
public class GetQuestionsToolTests
{
    private readonly Mock<IQuestionRepository> _mockRepository;
    private readonly GetQuestionsTool _tool;

    public GetQuestionsToolTests()
    {
        _mockRepository = new Mock<IQuestionRepository>();
        _tool = new GetQuestionsTool(_mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_ReturnsQuestions()
    {
        // Arrange
        var questions = new List<Question>
        {
            new() { Id = "q-1", QuestionText = "Test?" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(questions);

        var input = JsonDocument.Parse(@"{""limit"": 10}").RootElement;

        // Act
        var result = await _tool.ExecuteAsync(input, "user-123", CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("q-1", result.Content);
        _mockRepository.Verify(r => r.GetAllAsync("user-123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var input = JsonDocument.Parse("{}").RootElement;

        // Act
        var result = await _tool.ExecuteAsync(input, "user-123", CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Database error", result.Error);
    }
}
```

### Integration Testing

**Testing the full agent flow:**

```csharp
[Collection("IntegrationTests")]
public class AgentExecutorIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    [Fact]
    public async Task ExecuteTaskAsync_SimpleTask_CompletesSuccessfully()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<IAgentExecutor>();

        // Act
        var result = await executor.ExecuteTaskAsync(
            task: "Get all categories",
            userId: "test-user",
            CancellationToken.None
        );

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.Result);
    }
}
```

### Testing Best Practices

1. **Mock External Dependencies**
   - Always mock repositories
   - Don't call real Firestore in unit tests
   - Don't call Claude API in unit tests

2. **Test Edge Cases**
   - Empty input
   - Missing required fields
   - Invalid IDs
   - Null values

3. **Test Error Handling**
   - Repository exceptions
   - Invalid JSON
   - Timeout scenarios

4. **Test Security**
   - Verify userId is used in all repository calls
   - Test unauthorized access attempts

---

## Troubleshooting

### Common Issues

#### 1. "Tool not found in registry"

**Cause:** Tool not registered in DI
**Solution:** Add to `AgentModuleExtensions.cs`:
```csharp
services.AddScoped<IAgentTool, YourNewTool>();
```

#### 2. "Task exceeded timeout of 120 seconds"

**Cause:** Task taking too long
**Solutions:**
- Increase timeout in configuration
- Optimize tool implementations
- Break task into smaller subtasks

#### 3. "Anthropic API key is required"

**Cause:** Missing or invalid API key
**Solution:** Check `appsettings.Development.json`:
```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-api03-..."  // Must start with sk-ant-
  }
}
```

#### 4. "Max iterations reached"

**Cause:** Agent stuck in loop or task too complex
**Solutions:**
- Increase `MaxIterations` in configuration
- Improve system prompt clarity
- Simplify task description

#### 5. Hangfire jobs not processing

**Cause:** Hangfire server not running or no workers
**Solutions:**
- Check `AddHangfireServer()` is called in `Program.cs`
- Verify `WorkerCount > 0` in configuration
- Check Hangfire dashboard: `/hangfire`

### Debugging Tips

**Enable Detailed Logging:**

```json
{
  "Logging": {
    "LogLevel": {
      "QuestionRandomizer.Modules.Agent": "Debug",
      "Hangfire": "Information"
    }
  }
}
```

**Check Agent Execution Logs:**
```
[AgentExecutor] Starting agent task {TaskId} for user {UserId}
[AgentExecutor] Agent iteration 1/20
[AgentExecutor] Executing tool: get_questions
[AgentExecutor] Tool get_questions executed successfully
[AgentExecutor] Agent task {TaskId} completed successfully after 3 iterations
```

**Check Hangfire Logs:**
```
[Hangfire] Processing job {JobId}
[AgentTaskProcessor] Processing background agent task {TaskId}
[AgentTaskProcessor] Background agent task {TaskId} completed successfully
```

**Common Log Patterns:**

- **Timeout:** `Agent task {TaskId} exceeded timeout of {Timeout} seconds`
- **Tool Error:** `Error executing tool {ToolName}: {Error}`
- **Max Iterations:** `Agent task {TaskId} reached max iterations (20)`

---

## Examples

### Example 1: Categorize Uncategorized Questions

**User Request:**
```
POST /api/agent/queue
{
  "task": "Categorize all uncategorized questions about programming"
}
```

**Agent Flow:**
1. Calls `get_uncategorized_questions`
2. Receives list of 15 questions
3. Calls `get_categories` to see available categories
4. For each question:
   - Analyzes question text
   - Determines appropriate category
   - Calls `update_question_category`
5. Returns summary: "Successfully categorized 15 questions"

**Result:**
```json
{
  "taskId": "task-123",
  "status": "completed",
  "result": "Successfully categorized 15 questions:\n- 8 questions → JavaScript category\n- 5 questions → Python category\n- 2 questions → General Programming category"
}
```

---

### Example 2: Find and Merge Duplicates

**User Request:**
```
POST /api/agent/queue
{
  "task": "Find duplicate questions and delete the redundant ones"
}
```

**Agent Flow:**
1. Calls `find_duplicate_questions`
2. Receives groups of similar questions
3. For each group:
   - Analyzes which question is better (clarity, conciseness)
   - Calls `delete_question` for redundant ones
4. Returns summary with actions taken

**Result:**
```json
{
  "status": "completed",
  "result": "Found 6 duplicate groups, deleted 8 redundant questions:\n- 'What is a closure?' vs 'Explain closures' → Kept first, deleted second\n- 'How does async work?' vs 'Explain async/await' → Kept second (more specific)"
}
```

---

### Example 3: Batch Create Questions

**User Request:**
```
POST /api/agent/queue
{
  "task": "Create 5 sample JavaScript questions about closures for Senior Developer qualification"
}
```

**Agent Flow:**
1. Calls `get_qualifications` to find "Senior Developer" ID
2. Calls `get_categories` to find "JavaScript" category ID
3. Generates 5 questions about closures
4. For each question:
   - Calls `create_question` with appropriate categoryId and qualificationId
5. Returns summary

**Result:**
```json
{
  "status": "completed",
  "result": "Successfully created 5 JavaScript closure questions for Senior Developer qualification:\n1. Explain how closures maintain access to outer scope variables\n2. What are common use cases for closures in JavaScript?\n3. How do closures relate to the concept of lexical scoping?\n4. Describe a situation where closures might cause memory leaks\n5. How can you use closures to create private variables in JavaScript?"
}
```

---

### Example 4: Analyze Question Difficulty

**User Request:**
```
POST /api/agent/queue
{
  "task": "Analyze all Python questions and suggest difficulty levels"
}
```

**Agent Flow:**
1. Calls `search_questions` with "Python" to find relevant questions
2. Calls `analyze_question_difficulty` with Python categoryId
3. Reviews AI suggestions
4. Optionally updates questions with difficulty metadata
5. Returns analysis summary

**Result:**
```json
{
  "status": "completed",
  "result": "Analyzed 42 Python questions:\n- Easy (15): Basic syntax, variables, data types\n- Intermediate (20): Functions, OOP, list comprehensions\n- Hard (7): Decorators, metaclasses, async programming\n\nSuggestion: Add more hard-level questions for senior positions"
}
```

---

## Best Practices

### 1. Task Description Quality

**Good Task Descriptions:**
- Clear and specific
- Include relevant context
- Specify expected outcomes

**Examples:**

✅ **GOOD:** "Categorize all uncategorized JavaScript questions into the JavaScript category"
❌ **BAD:** "Fix the questions"

✅ **GOOD:** "Find duplicate questions with >80% similarity and delete the less clear ones"
❌ **BAD:** "Clean up duplicates"

✅ **GOOD:** "Create 10 intermediate-level TypeScript questions about generics for Senior Frontend Developer qualification"
❌ **BAD:** "Make some TypeScript questions"

### 2. Security Considerations

**Always:**
- Validate all agent outputs before using in production
- Monitor agent task execution logs
- Use userId filtering on all data operations
- Set appropriate timeout limits
- Review batch operations before execution

**Never:**
- Trust agent output blindly for critical operations
- Allow agents to perform irreversible actions without confirmation
- Use production data in development without sanitization

### 3. Performance Optimization

**For Large Datasets:**
- Use `limit` parameter in retrieval tools
- Implement pagination for large result sets
- Consider batch operations for bulk updates
- Monitor execution time and adjust timeouts

**For Complex Tasks:**
- Break into smaller subtasks
- Increase timeout for known long-running operations
- Use appropriate Claude model (Sonnet for balance, Opus for complexity)

### 4. Monitoring and Observability

**Key Metrics to Track:**
- Task completion rate
- Average execution time
- Tool usage frequency
- Retry counts
- Timeout occurrences

**Recommended Monitoring:**
```csharp
// Log important metrics
_logger.LogInformation(
    "Agent task completed: TaskId={TaskId}, Duration={Duration}ms, ToolCalls={ToolCalls}, Iterations={Iterations}",
    taskId, duration, toolCallCount, iterations
);
```

---

## Additional Resources

### Related Documentation

- **Main Backend Guide:** `../../CLAUDE.md`
- **Architecture Overview:** `../../../ARCHITECTURE.md` (coordination repo)
- **Migration Summary:** `../../MIGRATION-SUMMARY.md`
- **Code Templates:** `../../../docs/CODE-TEMPLATES.md`

### External Resources

- **Anthropic SDK Documentation:** https://github.com/Anthropic/anthropic-sdk-dotnet
- **Claude API Reference:** https://docs.anthropic.com/
- **Hangfire Documentation:** https://www.hangfire.io/
- **Firestore .NET Guide:** https://cloud.google.com/firestore/docs/client/libraries

### Support

**For Agent Module issues:**
1. Check this documentation
2. Review logs (Hangfire dashboard + application logs)
3. Check `TROUBLESHOOTING.md` section above
4. Review tool implementations for examples

---

**Last Updated:** 2025-12-27
**Module Version:** 1.0.0 (Phase 3 Complete)
**Status:** Production Ready
