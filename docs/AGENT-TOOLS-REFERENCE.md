# Agent Tools Reference

**Last Updated:** 2025-12-27

Complete reference documentation for all 15 AI agent tools in the Question Randomizer Agent Module.

---

## Overview

The Agent Module includes 15 specialized tools organized into 3 categories:

- **Data Retrieval (6 tools)** - Reading data from Firestore
- **Data Modification (7 tools)** - Creating, updating, deleting data
- **Data Analysis (2 tools)** - Analyzing and processing data

---

## Data Retrieval Tools

### 1. get_questions

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

**Output Example:**
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

**Common Use:** "Show me all my questions about JavaScript"

---

### 2. get_question_by_id

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

**Output Example:**
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

**Common Use:** "Get details for question q-123"

---

### 3. get_categories

**Purpose:** Retrieve all categories

**Input Schema:**
```json
{
  "type": "object",
  "properties": {}
}
```

**Output Example:**
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

**Common Use:** "What categories are available?"

---

### 4. get_qualifications

**Purpose:** Retrieve all qualifications

**Input Schema:**
```json
{
  "type": "object",
  "properties": {}
}
```

**Output Example:**
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

**Common Use:** "List all job qualifications"

---

### 5. get_uncategorized_questions

**Purpose:** Find questions without a category assigned

**Input Schema:**
```json
{
  "type": "object",
  "properties": {}
}
```

**Output Example:**
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

**Common Use:** "Find all questions that need categorization"

**Agent Workflow:**
```
Task: "Categorize all uncategorized questions"
1. Call get_uncategorized_questions
2. Analyze each question
3. Call update_question_category for each
```

---

### 6. search_questions

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

**Output Example:**
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

**Common Use:** "Find all questions about closures"

---

## Data Modification Tools

### 7. create_question

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

**Output Example:**
```json
{
  "id": "q-new-123",
  "questionText": "What is hoisting?",
  "categoryId": "cat-456",
  "message": "Question created successfully"
}
```

**Common Use:** "Create a new JavaScript question about hoisting"

---

### 8. update_question

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

**Output Example:**
```json
{
  "id": "q-123",
  "message": "Question updated successfully"
}
```

**Common Use:** "Update question q-123 text to 'Explain closures in detail'"

---

### 9. delete_question

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

**Output Example:**
```json
{
  "id": "q-123",
  "message": "Question deleted successfully"
}
```

**Common Use:** "Delete question q-123"

---

### 10. update_question_category

**Purpose:** Update only the category of a question (optimized for batch operations)

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

**Output Example:**
```json
{
  "id": "q-123",
  "categoryId": "cat-456",
  "message": "Question category updated successfully"
}
```

**Common Use:** "Move question q-123 to JavaScript category"

**Performance:** Optimized for batch categorization tasks

---

### 11. create_category

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

**Output Example:**
```json
{
  "id": "cat-new-123",
  "name": "TypeScript",
  "message": "Category created successfully"
}
```

**Common Use:** "Create a new category for TypeScript questions"

---

### 12. create_qualification

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

**Output Example:**
```json
{
  "id": "qual-new-123",
  "name": "Lead Developer",
  "message": "Qualification created successfully"
}
```

**Common Use:** "Create a qualification for Lead Developer position"

---

### 13. batch_update_questions

**Purpose:** Update multiple questions in one operation (optimized for performance)

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

**Output Example:**
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

**Common Use:** "Update categories for 50 questions at once"

**Performance:** Optimized for bulk operations, much faster than individual updates

---

## Data Analysis Tools

### 14. find_duplicate_questions

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

**Output Example:**
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

**Common Use:** "Find duplicate questions and suggest which to keep"

**Agent Workflow:**
```
Task: "Find and delete duplicate questions"
1. Call find_duplicate_questions
2. Review suggestions
3. Call delete_question for redundant ones
```

---

### 15. analyze_question_difficulty

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

**Output Example:**
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

**Common Use:** "Analyze difficulty of all Python questions"

---

## Tool Categories Summary

### Data Retrieval Tools
| Tool | Purpose | Required Input |
|------|---------|----------------|
| get_questions | List all questions | None |
| get_question_by_id | Get single question | questionId |
| get_categories | List categories | None |
| get_qualifications | List qualifications | None |
| get_uncategorized_questions | Find uncategorized | None |
| search_questions | Search by text | searchText |

### Data Modification Tools
| Tool | Purpose | Required Input |
|------|---------|----------------|
| create_question | Create new question | questionText |
| update_question | Update question | questionId |
| delete_question | Delete question | questionId |
| update_question_category | Update category only | questionId, categoryId |
| create_category | Create new category | name |
| create_qualification | Create new qualification | name |
| batch_update_questions | Bulk update | updates array |

### Data Analysis Tools
| Tool | Purpose | Required Input |
|------|---------|----------------|
| find_duplicate_questions | Find duplicates | None |
| analyze_question_difficulty | Analyze difficulty | None |

---

## Common Tool Combinations

### Categorization Workflow
```
1. get_uncategorized_questions → Find questions needing categories
2. get_categories → See available categories
3. update_question_category → Assign categories (one by one)
   OR
   batch_update_questions → Assign categories (bulk)
```

### Duplicate Management Workflow
```
1. find_duplicate_questions → Identify duplicates
2. get_question_by_id → Review each question in detail
3. delete_question → Remove redundant questions
```

### Question Creation Workflow
```
1. get_categories → See existing categories
2. get_qualifications → See available qualifications
3. create_question → Create question with category/qualification
```

### Data Cleanup Workflow
```
1. search_questions → Find questions with specific criteria
2. update_question → Fix individual questions
   OR
   batch_update_questions → Fix multiple questions
   OR
   delete_question → Remove unwanted questions
```

---

## Security Notes

**All tools enforce security:**
- ✅ userId filtering on all data operations
- ✅ No cross-user data access
- ✅ Soft deletes (no permanent data loss)
- ✅ Audit logging in Firestore

**Tool execution is secure by design:**
```csharp
// Every tool automatically receives userId
protected override async Task<ToolResult> ExecuteInternalAsync(
    JsonElement input,
    string userId,  // ← Always provided, always used
    CancellationToken cancellationToken)
{
    // userId MUST be used in all repository calls
    var data = await _repository.GetAsync(userId, ...);
}
```

---

## Related Documentation

- **[AGENT-TOOL-DEVELOPMENT.md](./AGENT-TOOL-DEVELOPMENT.md)** - How to create new tools
- **[AGENT-EXAMPLES.md](./AGENT-EXAMPLES.md)** - Real-world usage examples
- **[Module CLAUDE.md](../src/Modules/QuestionRandomizer.Modules.Agent/CLAUDE.md)** - Agent Module overview

---

**Last Updated:** 2025-12-27
**Tool Count:** 15 (6 retrieval + 7 modification + 2 analysis)
