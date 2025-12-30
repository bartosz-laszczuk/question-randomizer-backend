# Agent Module Examples

**Last Updated:** 2025-12-30

Real-world examples of AI agent tasks and their execution workflows.

---

## Example 1: Categorize Uncategorized Questions

### User Request
```http
POST /api/agent/execute
{
  "task": "Categorize all uncategorized questions about programming"
}
```

### Agent Execution Flow

**Step 1:** Agent analyzes the task
- Understands it needs to categorize questions
- Decides to find uncategorized questions first

**Step 2:** Calls `get_uncategorized_questions`
```json
Request: {}
Response: {
  "count": 15,
  "questions": [
    { "id": "q-1", "questionText": "What is a closure in JavaScript?" },
    { "id": "q-2", "questionText": "Explain Python decorators" },
    { "id": "q-3", "questionText": "How does async/await work?" },
    ...
  ]
}
```

**Step 3:** Calls `get_categories`
```json
Request: {}
Response: {
  "categories": [
    { "id": "cat-js", "name": "JavaScript" },
    { "id": "cat-py", "name": "Python" },
    { "id": "cat-gen", "name": "General Programming" }
  ]
}
```

**Step 4:** Agent analyzes each question and assigns categories

For question "What is a closure in JavaScript?":
```json
Call: update_question_category
Request: {
  "questionId": "q-1",
  "categoryId": "cat-js"
}
Response: {
  "id": "q-1",
  "categoryId": "cat-js",
  "message": "Question category updated successfully"
}
```

**Step 5:** Repeats for all 15 questions

**Final Response:**
```json
{
  "taskId": "task-123",
  "status": "completed",
  "result": "Successfully categorized 15 questions:\n- 8 questions → JavaScript category\n- 5 questions → Python category\n- 2 questions → General Programming category"
}
```

---

## Example 2: Find and Merge Duplicates

### User Request
```http
POST /api/agent/execute
{
  "task": "Find duplicate questions and delete the redundant ones, keeping the better-worded version"
}
```

### Agent Execution Flow

**Step 1:** Calls `find_duplicate_questions`
```json
Request: { "similarityThreshold": 0.8 }
Response: {
  "duplicateGroups": [
    {
      "similarity": 0.95,
      "questions": [
        { "id": "q-100", "questionText": "What is a closure?" },
        { "id": "q-200", "questionText": "Explain closures in JavaScript" }
      ],
      "suggestion": "Keep q-100, delete q-200 (more concise)"
    },
    {
      "similarity": 0.92,
      "questions": [
        { "id": "q-300", "questionText": "How does async work?" },
        { "id": "q-400", "questionText": "Explain async/await in JavaScript" }
      ],
      "suggestion": "Keep q-400, delete q-300 (more specific)"
    }
  ]
}
```

**Step 2:** Agent reviews each group

For first group (q-100 vs q-200):
- Analyzes both questions
- Agrees with suggestion to keep q-100 (more concise)
- Calls `delete_question` for q-200

```json
Call: delete_question
Request: { "questionId": "q-200" }
Response: { "id": "q-200", "message": "Question deleted successfully" }
```

For second group (q-300 vs q-400):
- Analyzes both questions
- Agrees with suggestion to keep q-400 (more specific)
- Calls `delete_question` for q-300

```json
Call: delete_question
Request: { "questionId": "q-300" }
Response: { "id": "q-300", "message": "Question deleted successfully" }
```

**Final Response:**
```json
{
  "status": "completed",
  "result": "Found 2 duplicate groups, deleted 2 redundant questions:\n\nGroup 1:\n- Kept: 'What is a closure?' (q-100)\n- Deleted: 'Explain closures in JavaScript' (q-200)\n- Reason: First version is more concise\n\nGroup 2:\n- Kept: 'Explain async/await in JavaScript' (q-400)\n- Deleted: 'How does async work?' (q-300)\n- Reason: Kept version is more specific"
}
```

---

## Example 3: Batch Create Questions

### User Request
```http
POST /api/agent/execute
{
  "task": "Create 5 JavaScript closure questions for Senior Developer qualification, make them challenging"
}
```

### Agent Execution Flow

**Step 1:** Calls `get_qualifications`
```json
Request: {}
Response: {
  "qualifications": [
    { "id": "qual-sr", "name": "Senior Developer" },
    { "id": "qual-jr", "name": "Junior Developer" }
  ]
}
```

**Step 2:** Calls `get_categories`
```json
Request: {}
Response: {
  "categories": [
    { "id": "cat-js", "name": "JavaScript" }
  ]
}
```

**Step 3:** Agent generates 5 challenging closure questions

**Question 1:**
```json
Call: create_question
Request: {
  "questionText": "Explain how closures maintain access to outer scope variables even after the outer function has returned. Provide a practical example.",
  "categoryId": "cat-js",
  "qualificationId": "qual-sr"
}
Response: {
  "id": "q-new-1",
  "message": "Question created successfully"
}
```

**Question 2:**
```json
Call: create_question
Request: {
  "questionText": "What are common use cases for closures in JavaScript? Describe at least three with examples.",
  "categoryId": "cat-js",
  "qualificationId": "qual-sr"
}
Response: {
  "id": "q-new-2",
  "message": "Question created successfully"
}
```

**Questions 3-5:** Similar pattern...

**Final Response:**
```json
{
  "status": "completed",
  "result": "Successfully created 5 JavaScript closure questions for Senior Developer qualification:\n\n1. Explain how closures maintain access to outer scope variables even after the outer function has returned. Provide a practical example.\n\n2. What are common use cases for closures in JavaScript? Describe at least three with examples.\n\n3. How do closures relate to the concept of lexical scoping? Explain the relationship.\n\n4. Describe a situation where closures might cause memory leaks. How would you prevent this?\n\n5. How can you use closures to create private variables in JavaScript? Demonstrate with code.\n\nAll questions are categorized under JavaScript and assigned to Senior Developer qualification."
}
```

---

## Example 4: Analyze Question Difficulty

### User Request
```http
POST /api/agent/execute
{
  "task": "Analyze all Python questions and suggest difficulty levels (easy, intermediate, hard)"
}
```

### Agent Execution Flow

**Step 1:** Calls `search_questions`
```json
Request: { "searchText": "Python", "limit": 100 }
Response: {
  "count": 42,
  "questions": [
    { "id": "q-1", "questionText": "What is a variable in Python?" },
    { "id": "q-2", "questionText": "Explain Python decorators" },
    { "id": "q-3", "questionText": "How do metaclasses work?" },
    ...
  ]
}
```

**Step 2:** Calls `analyze_question_difficulty`
```json
Request: { "categoryId": "cat-python" }
Response: {
  "analysis": [
    {
      "questionId": "q-1",
      "questionText": "What is a variable in Python?",
      "suggestedDifficulty": "easy",
      "reasoning": "Basic concept, fundamental knowledge",
      "confidence": 0.95
    },
    {
      "questionId": "q-2",
      "questionText": "Explain Python decorators",
      "suggestedDifficulty": "intermediate",
      "reasoning": "Requires understanding of functions and closures",
      "confidence": 0.88
    },
    {
      "questionId": "q-3",
      "questionText": "How do metaclasses work?",
      "suggestedDifficulty": "hard",
      "reasoning": "Advanced topic, requires deep Python knowledge",
      "confidence": 0.92
    }
  ],
  "summary": {
    "easy": 15,
    "intermediate": 20,
    "hard": 7
  }
}
```

**Step 3:** Agent reviews and summarizes findings

**Final Response:**
```json
{
  "status": "completed",
  "result": "Analyzed 42 Python questions:\n\nDifficulty Breakdown:\n- Easy (15 questions): Basic syntax, variables, data types, simple loops\n- Intermediate (20 questions): Functions, OOP, list comprehensions, error handling\n- Hard (7 questions): Decorators, metaclasses, async programming, memory management\n\nRecommendations:\n1. Good balance of easy and intermediate questions\n2. Consider adding more hard-level questions for senior positions\n3. Most confident classifications (>0.9 confidence): 35 questions\n4. Review these manually (lower confidence): q-15, q-23, q-31\n\nSuggested Next Steps:\n- Create 5-10 more advanced Python questions\n- Consider adding expert-level category for metaclasses/async topics"
}
```

---

## Example 5: Complex Multi-Step Task

### User Request
```http
POST /api/agent/execute
{
  "task": "Review all my questions: categorize any uncategorized ones, find and remove duplicates, and create a summary report"
}
```

### Agent Execution Flow

**Phase 1: Categorization**
1. Calls `get_uncategorized_questions`
2. Calls `get_categories`
3. Uses `batch_update_questions` for efficient categorization

**Phase 2: Duplicate Detection**
4. Calls `find_duplicate_questions`
5. Reviews each duplicate group
6. Calls `delete_question` for redundant ones

**Phase 3: Summary Report**
7. Calls `get_questions` with limit to get total count
8. Calls `get_categories` to count by category
9. Compiles comprehensive report

**Final Response:**
```json
{
  "status": "completed",
  "result": "Question Database Review Complete\n\nCategorization:\n- Found 12 uncategorized questions\n- Successfully categorized all 12\n  - JavaScript: 5\n  - Python: 4\n  - General: 3\n\nDuplicate Removal:\n- Identified 3 duplicate groups\n- Deleted 3 redundant questions\n- Kept better-worded versions\n\nCurrent Database Summary:\n- Total Questions: 147\n- Categories:\n  - JavaScript: 58\n  - Python: 42\n  - TypeScript: 25\n  - General Programming: 18\n  - Database: 4\n- All questions now properly categorized\n- No duplicates detected\n\nRecommendations:\n- Database looks healthy\n- Consider adding more Database category questions\n- Good distribution across other categories"
}
```

---

## Example 6: Error Recovery

### User Request (Intentional Error)
```http
POST /api/agent/execute
{
  "task": "Delete all questions in category cat-999"
}
```

### Agent Execution Flow

**Step 1:** Calls `search_questions`
```json
Request: { "searchText": "cat-999" }
Response: { "count": 0, "questions": [] }
```

**Step 2:** Calls `get_categories` to verify category exists
```json
Request: {}
Response: {
  "categories": [
    { "id": "cat-js", "name": "JavaScript" },
    { "id": "cat-py", "name": "Python" }
  ]
}
```

**Step 3:** Agent realizes category doesn't exist

**Final Response:**
```json
{
  "status": "completed",
  "result": "Unable to complete task: Category 'cat-999' does not exist.\n\nAvailable categories:\n- cat-js: JavaScript\n- cat-py: Python\n\nPlease verify the category ID and try again."
}
```

**Key Point:** Agent handles errors gracefully and provides helpful feedback

---

## Common Task Patterns

### Pattern 1: Find → Analyze → Act
```
Example: "Find outdated questions and update them"
1. search_questions (find candidates)
2. Agent analyzes each
3. update_question (update as needed)
```

### Pattern 2: Bulk Operations
```
Example: "Categorize 50 questions"
1. get_uncategorized_questions
2. get_categories
3. batch_update_questions (efficient bulk update)
```

### Pattern 3: Data Cleanup
```
Example: "Clean up my question database"
1. find_duplicate_questions
2. delete_question (remove duplicates)
3. get_uncategorized_questions
4. update_question_category (categorize)
```

### Pattern 4: Report Generation
```
Example: "Analyze my question database"
1. get_questions
2. analyze_question_difficulty
3. get_categories
4. Compile comprehensive report
```

---

## Best Practices for Task Descriptions

### ✅ Good Task Descriptions

**Specific and Clear:**
```
"Categorize all uncategorized JavaScript questions into the JavaScript category"
"Find questions with similarity > 80% and delete the less clear ones"
"Create 10 intermediate TypeScript questions about generics for Senior Frontend position"
```

**Includes Context:**
```
"Review Python questions and suggest which are too easy for Senior Developer role"
"Find duplicate questions, keeping the version with better grammar"
"Create challenging algorithm questions suitable for technical interviews"
```

### ❌ Poor Task Descriptions

**Too Vague:**
```
"Fix the questions"  ← Fix what? How?
"Clean up database"  ← Clean what specifically?
"Make some questions"  ← How many? What topic? What difficulty?
```

**Ambiguous:**
```
"Delete duplicates"  ← Which one to keep? Based on what criteria?
"Update questions"  ← Which questions? Update what fields?
```

---

## Task Execution Times

Typical execution times for common tasks:

| Task Type | Example | Typical Time |
|-----------|---------|--------------|
| Simple Retrieval | "List all categories" | 2-5 seconds |
| Single Update | "Update question q-123" | 3-8 seconds |
| Categorization (10 items) | "Categorize 10 questions" | 15-30 seconds |
| Duplicate Detection | "Find duplicates in 100 questions" | 20-40 seconds |
| Batch Creation | "Create 5 new questions" | 25-45 seconds |
| Complex Multi-Step | "Review and clean database" | 45-90 seconds |

**Note:** Times vary based on:
- Number of tool calls required
- Complexity of analysis
- Firestore query performance
- Claude API response time

---

## Monitoring and Debugging

### Real-Time Streaming Events

With the streaming API, you receive real-time updates as the task executes:

```csharp
await foreach (var streamEvent in _agentService.ExecuteTaskStreamingAsync(
    task: "Categorize all questions",
    userId: currentUserId,
    cancellationToken: cancellationToken))
{
    switch (streamEvent.Type)
    {
        case "started":
            Console.WriteLine("Task started");
            break;
        case "thinking":
            Console.WriteLine("Agent is analyzing...");
            break;
        case "tool_call":
            Console.WriteLine($"Calling tool: {streamEvent.ToolName}");
            break;
        case "tool_result":
            Console.WriteLine($"Tool result: {streamEvent.Content}");
            break;
        case "text_chunk":
            Console.Write(streamEvent.Content);
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

### View Execution Logs

Check application logs for agent execution details:
```
[AgentExecutor] Starting streaming execution
[AgentExecutor] Agent iteration 1/20
[AgentExecutor] Executing tool: get_uncategorized_questions
[AgentExecutor] Tool executed successfully
[AgentExecutor] Agent iteration 2/20
[AgentExecutor] Executing tool: update_question_category
...
[AgentExecutor] Agent task completed after 5 iterations
```

---

## Example 7: 🆕 Conversational Multi-Turn Task

### User Request (Turn 1 - Start Conversation)
```http
POST /api/agent/execute
{
  "task": "Update all uncategorized questions"
}

Response: {
  "taskId": "task-789",
  "message": "Task queued for processing"
}
```

### Agent Execution (Turn 1)
- **No conversationId** provided → Creates new conversation "conv-123"
- **No conversation history** → Agent starts fresh
- Saves user message: "Update all uncategorized questions"
- Executes task:
  1. Calls `get_uncategorized_questions` → Finds 15 questions
  2. Calls `get_categories` → Gets available categories
  3. Calls `update_question_category` 15 times → Categorizes each question
- Agent response: "✅ Successfully updated 15 questions: q-1, q-2, q-3, q-4, q-5, q-6, q-7, q-8, q-9, q-10, q-11, q-12, q-13, q-14, q-15"
- Saves assistant message to conversation "conv-123"

### User Request (Turn 2 - Continue Conversation)
```http
POST /api/agent/execute
{
  "task": "Provide me the ids of all updated questions",
  "conversationId": "conv-123"
}
```

### Agent Execution (Turn 2)
- **conversationId: "conv-123"** → Loads conversation history:
  ```
  Turn 1 User: "Update all uncategorized questions"
  Turn 1 Agent: "✅ Successfully updated 15 questions: q-1, q-2, q-3..."
  ```
- **Agent sees FULL CONTEXT!** Knows about the 15 updated questions from Turn 1
- Saves new user message: "Provide me the ids of all updated questions"
- Agent analyzes:
  - "The user is asking for the question IDs"
  - "I already mentioned them in my previous response"
  - "The IDs are: q-1, q-2, q-3, q-4, q-5, q-6, q-7, q-8, q-9, q-10, q-11, q-12, q-13, q-14, q-15"
- Agent response: "The updated question IDs are: q-1, q-2, q-3, q-4, q-5, q-6, q-7, q-8, q-9, q-10, q-11, q-12, q-13, q-14, q-15"
- Saves assistant message to conversation "conv-123"

### User Request (Turn 3 - Continue Conversation)
```http
POST /api/agent/execute
{
  "task": "Delete the first 3 questions",
  "conversationId": "conv-123"
}
```

### Agent Execution (Turn 3)
- **conversationId: "conv-123"** → Loads full conversation history:
  ```
  Turn 1 User: "Update all uncategorized questions"
  Turn 1 Agent: "✅ Successfully updated 15 questions: q-1, q-2, q-3..."
  Turn 2 User: "Provide me the ids of all updated questions"
  Turn 2 Agent: "The updated question IDs are: q-1, q-2, q-3, q-4..."
  ```
- **Agent sees FULL CONTEXT!** Knows:
  - Which 15 questions were updated
  - Their IDs: q-1 through q-15
  - "First 3" means q-1, q-2, q-3
- Saves new user message: "Delete the first 3 questions"
- Executes task:
  1. Calls `delete_question` for q-1
  2. Calls `delete_question` for q-2
  3. Calls `delete_question` for q-3
- Agent response: "✅ Successfully deleted questions q-1, q-2, and q-3"
- Saves assistant message to conversation "conv-123"

### Key Points

**Conversational Context Benefits:**
- ✅ Agent remembers ALL previous messages (user + assistant)
- ✅ Can reference previous responses ("the first 3" → q-1, q-2, q-3)
- ✅ Natural multi-turn interaction like ChatGPT
- ✅ No need to repeat information across turns

**Implementation Details:**
- **Automatic conversation creation** when conversationId is null
- **Conversation persistence** in Firestore (conversations + messages collections)
- **Secure by design** - All conversations filtered by userId
- **Works everywhere** - Queue, execute, and streaming endpoints

**Use Cases:**
- Multi-step workflows
- Iterative refinement
- Follow-up questions
- Context-dependent operations

**Comparison:**

| Without conversationId | With conversationId |
|----------------------|-------------------|
| One-shot task | Multi-turn conversation |
| No context | Full conversation history |
| Agent starts fresh | Agent remembers everything |
| For standalone tasks | For related follow-ups |

---

## Related Documentation

- **[AGENT-TOOLS-REFERENCE.md](./AGENT-TOOLS-REFERENCE.md)** - Complete reference of all 15 tools
- **[AGENT-TOOL-DEVELOPMENT.md](./AGENT-TOOL-DEVELOPMENT.md)** - How to create new tools
- **[Module CLAUDE.md](../src/Modules/QuestionRandomizer.Modules.Agent/CLAUDE.md)** - Agent Module overview

---

**Last Updated:** 2025-12-30
