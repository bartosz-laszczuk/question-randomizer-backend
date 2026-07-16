# Agent Tool Development Guide

**Last Updated:** 2025-12-27

Complete guide for developing new AI agent tools for the Question Randomizer Agent Module.

---

## Table of Contents

1. [Creating a New Tool](#creating-a-new-tool)
2. [Tool Development Workflow](#tool-development-workflow)
3. [Base Classes and Interfaces](#base-classes-and-interfaces)
4. [Input Schema Definition](#input-schema-definition)
5. [Best Practices](#best-practices)
6. [Testing Tools](#testing-tools)

---

## Creating a New Tool

### Step 1: Create Tool Class

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

### Step 2: Register Tool in DI

Add to `AgentModuleExtensions.cs`:

```csharp
// Data Retrieval Tools
services.AddScoped<IAgentTool, GetQuestionsTool>();
services.AddScoped<IAgentTool, GetQuestionByIdTool>();
services.AddScoped<IAgentTool, GetQuestionsByQualificationTool>(); // NEW TOOL
```

### Step 3: Test Your Tool

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

---

## Tool Development Workflow

### Complete Workflow

```
1. Identify Need
   ↓
2. Create Tool Class
   ↓
3. Define Name, Description, Schema
   ↓
4. Implement ExecuteInternalAsync
   ↓
5. Register in DI
   ↓
6. Write Unit Tests
   ↓
7. Test Integration with Agent
   ↓
8. Deploy
```

### Development Checklist

- [ ] Tool class created in appropriate folder
- [ ] Inherits from `AgentToolBase`
- [ ] Name is clear and descriptive (snake_case)
- [ ] Description explains purpose clearly
- [ ] Input schema is complete and valid JSON
- [ ] Required fields marked in schema
- [ ] ExecuteInternalAsync implemented
- [ ] Input validation included
- [ ] userId used for data filtering
- [ ] Error handling in place
- [ ] Registered in `AgentModuleExtensions.cs`
- [ ] Unit tests written
- [ ] Integration tested with agent

---

## Base Classes and Interfaces

### IAgentTool Interface

```csharp
public interface IAgentTool
{
    /// <summary>
    /// Unique tool name (snake_case, e.g., "get_questions")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Human-readable description for the AI
    /// </summary>
    string Description { get; }

    /// <summary>
    /// JSON Schema defining expected input structure
    /// </summary>
    string InputSchemaJson { get; }

    /// <summary>
    /// Execute the tool with given input
    /// </summary>
    Task<ToolResult> ExecuteAsync(
        JsonElement input,
        string userId,
        CancellationToken cancellationToken);
}
```

### AgentToolBase Abstract Class

Provides common functionality:

```csharp
public abstract class AgentToolBase : IAgentTool
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string InputSchemaJson { get; }

    // Public method with error handling
    public async Task<ToolResult> ExecuteAsync(
        JsonElement input,
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ExecuteInternalAsync(input, userId, cancellationToken);
        }
        catch (Exception ex)
        {
            return ToolResult.Failure($"Tool execution failed: {ex.Message}");
        }
    }

    // Override this in your tool
    protected abstract Task<ToolResult> ExecuteInternalAsync(
        JsonElement input,
        string userId,
        CancellationToken cancellationToken);
}
```

### ToolResult Class

```csharp
public class ToolResult
{
    public bool Success { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Error { get; set; }

    public static ToolResult Success(string content) =>
        new() { Success = true, Content = content };

    public static ToolResult Failure(string error) =>
        new() { Success = false, Error = error };
}
```

---

## Input Schema Definition

### JSON Schema Format

Tools use JSON Schema (Draft 7) to define expected input:

```json
{
  "type": "object",
  "properties": {
    "propertyName": {
      "type": "string",
      "description": "What this property is for"
    }
  },
  "required": ["propertyName"]
}
```

### Supported Types

**String:**
```json
{
  "propertyName": {
    "type": "string",
    "description": "A text value"
  }
}
```

**Number:**
```json
{
  "count": {
    "type": "number",
    "description": "Numeric value"
  }
}
```

**Boolean:**
```json
{
  "includeDeleted": {
    "type": "boolean",
    "description": "Whether to include deleted items"
  }
}
```

**Array:**
```json
{
  "questionIds": {
    "type": "array",
    "description": "List of question IDs",
    "items": {
      "type": "string"
    }
  }
}
```

**Object:**
```json
{
  "filters": {
    "type": "object",
    "description": "Filter criteria",
    "properties": {
      "categoryId": { "type": "string" },
      "difficulty": { "type": "string" }
    }
  }
}
```

### Schema Best Practices

**1. Clear Descriptions**
```json
✅ GOOD:
{
  "questionId": {
    "type": "string",
    "description": "The unique identifier of the question to retrieve"
  }
}

❌ BAD:
{
  "questionId": {
    "type": "string",
    "description": "ID"
  }
}
```

**2. Required Fields**
```json
{
  "type": "object",
  "properties": {
    "questionId": { "type": "string" },
    "includeDeleted": { "type": "boolean" }
  },
  "required": ["questionId"]  // Only questionId is required
}
```

**3. Default Values**
Handle defaults in code, not schema:
```csharp
var limit = input.TryGetProperty("limit", out var limitProp)
    ? limitProp.GetInt32()
    : 100; // Default to 100
```

---

## Best Practices

### 1. Input Validation

**Always validate required fields:**
```csharp
var questionId = input.GetProperty("questionId").GetString();
if (string.IsNullOrEmpty(questionId))
{
    return ToolResult.Failure("questionId is required");
}
```

**Validate format:**
```csharp
if (!Guid.TryParse(questionId, out _))
{
    return ToolResult.Failure("questionId must be a valid GUID");
}
```

### 2. Security - Always Use userId

**ALWAYS filter by userId:**
```csharp
✅ GOOD:
var questions = await _questionRepository.GetAllAsync(userId, cancellationToken);

❌ BAD - Security vulnerability!
var questions = await _questionRepository.GetAllAsync();
```

**Verify ownership before modifications:**
```csharp
var question = await _questionRepository.GetByIdAsync(questionId, userId, cancellationToken);
if (question == null)
{
    return ToolResult.Failure("Question not found or access denied");
}
```

### 3. Error Handling

**Return helpful error messages:**
```csharp
✅ GOOD:
return ToolResult.Failure("Category 'cat-123' not found. Available categories: JavaScript, Python, General");

❌ BAD:
return ToolResult.Failure("Error");
```

**Catch specific exceptions:**
```csharp
try
{
    var result = await _repository.DeleteAsync(id, userId, cancellationToken);
    return ToolResult.Success(JsonSerializer.Serialize(result));
}
catch (NotFoundException ex)
{
    return ToolResult.Failure($"Question not found: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    return ToolResult.Failure($"Cannot delete: {ex.Message}");
}
```

### 4. Performance

**Limit result sizes:**
```csharp
var limit = input.TryGetProperty("limit", out var limitProp)
    ? Math.Min(limitProp.GetInt32(), 1000) // Cap at 1000
    : 100;
```

**Use pagination for large datasets:**
```csharp
var result = new
{
    questions = questions.Take(limit),
    total = totalCount,
    hasMore = totalCount > limit,
    nextOffset = limit
};
```

### 5. Output Format

**Return structured JSON:**
```csharp
✅ GOOD:
var result = new
{
    success = true,
    count = questions.Count,
    questions = questions.Select(q => new { id = q.Id, text = q.QuestionText })
};
return ToolResult.Success(JsonSerializer.Serialize(result));

❌ BAD:
return ToolResult.Success($"Found {questions.Count} questions");
```

**Include metadata:**
```csharp
var result = new
{
    count = questions.Count,
    categoryBreakdown = questions.GroupBy(q => q.CategoryId)
        .Select(g => new { categoryId = g.Key, count = g.Count() }),
    questions = questions
};
```

---

## Testing Tools

### Unit Testing Pattern

```csharp
public class MyToolTests
{
    private readonly Mock<IRepository> _mockRepository;
    private readonly MyTool _tool;

    public MyToolTests()
    {
        _mockRepository = new Mock<IRepository>();
        _tool = new MyTool(_mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Entity());

        var input = JsonDocument.Parse(@"{""id"": ""123""}").RootElement;

        // Act
        var result = await _tool.ExecuteAsync(input, "user-123", CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredField_ReturnsFailure()
    {
        // Arrange
        var input = JsonDocument.Parse("{}").RootElement;

        // Act
        var result = await _tool.ExecuteAsync(input, "user-123", CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var input = JsonDocument.Parse(@"{""id"": ""123""}").RootElement;

        // Act
        var result = await _tool.ExecuteAsync(input, "user-123", CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Database error", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_UsesProvidedUserId()
    {
        // Arrange
        var input = JsonDocument.Parse(@"{""id"": ""123""}").RootElement;

        // Act
        await _tool.ExecuteAsync(input, "user-456", CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.GetAsync("user-456", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### Integration Testing

Test tools with the full agent:

```csharp
[Collection("IntegrationTests")]
public class ToolIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Agent_CanUseNewTool_Successfully()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<IAgentExecutor>();

        // Act
        var result = await executor.ExecuteTaskAsync(
            task: "Get questions for qualification qual-123",
            userId: "test-user",
            CancellationToken.None
        );

        // Assert
        Assert.True(result.Success);
        Assert.Contains("questions", result.Result);
    }
}
```

---

## Common Patterns

### Pattern 1: Simple Retrieval Tool

```csharp
public class GetItemTool : AgentToolBase
{
    private readonly IRepository _repository;

    public override string Name => "get_item";
    public override string Description => "Gets an item by ID";
    public override string InputSchemaJson => @"{
        ""type"": ""object"",
        ""properties"": {
            ""itemId"": { ""type"": ""string"", ""description"": ""Item ID"" }
        },
        ""required"": [""itemId""]
    }";

    protected override async Task<ToolResult> ExecuteInternalAsync(
        JsonElement input, string userId, CancellationToken cancellationToken)
    {
        var itemId = input.GetProperty("itemId").GetString()!;
        var item = await _repository.GetByIdAsync(itemId, userId, cancellationToken);

        if (item == null)
            return ToolResult.Failure("Item not found");

        return ToolResult.Success(JsonSerializer.Serialize(item));
    }
}
```

### Pattern 2: List with Filtering

```csharp
public class GetItemsTool : AgentToolBase
{
    public override string InputSchemaJson => @"{
        ""type"": ""object"",
        ""properties"": {
            ""categoryId"": { ""type"": ""string"" },
            ""limit"": { ""type"": ""number"" }
        }
    }";

    protected override async Task<ToolResult> ExecuteInternalAsync(
        JsonElement input, string userId, CancellationToken cancellationToken)
    {
        var categoryId = input.TryGetProperty("categoryId", out var cat)
            ? cat.GetString()
            : null;
        var limit = input.TryGetProperty("limit", out var lim)
            ? lim.GetInt32()
            : 100;

        var items = await _repository.GetAsync(userId, categoryId, limit, cancellationToken);

        return ToolResult.Success(JsonSerializer.Serialize(new
        {
            count = items.Count,
            items
        }));
    }
}
```

### Pattern 3: Modification Tool

```csharp
public class UpdateItemTool : AgentToolBase
{
    public override string InputSchemaJson => @"{
        ""type"": ""object"",
        ""properties"": {
            ""itemId"": { ""type"": ""string"" },
            ""name"": { ""type"": ""string"" }
        },
        ""required"": [""itemId""]
    }";

    protected override async Task<ToolResult> ExecuteInternalAsync(
        JsonElement input, string userId, CancellationToken cancellationToken)
    {
        var itemId = input.GetProperty("itemId").GetString()!;

        // Verify ownership
        var item = await _repository.GetByIdAsync(itemId, userId, cancellationToken);
        if (item == null)
            return ToolResult.Failure("Item not found");

        // Update fields
        if (input.TryGetProperty("name", out var name))
            item.Name = name.GetString()!;

        await _repository.UpdateAsync(item, cancellationToken);

        return ToolResult.Success(JsonSerializer.Serialize(new
        {
            id = item.Id,
            message = "Item updated successfully"
        }));
    }
}
```

---

## Troubleshooting

### Tool Not Found

**Error:** "Tool 'my_tool' not found in registry"

**Solution:** Check DI registration in `AgentModuleExtensions.cs`:
```csharp
services.AddScoped<IAgentTool, MyTool>();
```

### Schema Validation Errors

**Error:** Agent tries to call tool with wrong parameters

**Solution:** Validate schema JSON is valid:
```bash
# Use online JSON Schema validator
# Or validate in code during testing
```

### Tool Always Fails

**Solution:** Check error handling:
```csharp
// Add logging to see actual error
protected override async Task<ToolResult> ExecuteInternalAsync(...)
{
    try
    {
        // Your code
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Tool execution failed");
        return ToolResult.Failure($"Error: {ex.Message}");
    }
}
```

---

## Related Documentation

- **[AGENT-TOOLS-REFERENCE.md](./AGENT-TOOLS-REFERENCE.md)** - Complete reference of all 15 tools
- **[AGENT-EXAMPLES.md](./AGENT-EXAMPLES.md)** - Real-world usage examples
- **[TESTING.md](./TESTING.md)** - General testing guide
- **[CODE-TEMPLATES.md](./CODE-TEMPLATES.md)** - Other code patterns

---

**Last Updated:** 2025-12-27
