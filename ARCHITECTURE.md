# Question Randomizer - System Architecture

**System Name:** Question Randomizer
**Type:** Interview preparation tool with AI-powered assistant
**Architecture:** Microservices with separation of concerns
**Last Updated:** 2025-11-22

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Architecture Diagram](#architecture-diagram)
3. [Components](#components)
4. [Technology Stack](#technology-stack)
5. [Database Schema](#database-schema)
6. [Data Flow](#data-flow)
7. [API Design](#api-design)
8. [Agent Service Design](#agent-service-design)
9. [Database Access Patterns](#database-access-patterns)
10. [Security Architecture](#security-architecture)
11. [Scalability Considerations](#scalability-considerations)
12. [Deployment Architecture](#deployment-architecture)
13. [Repository Structure](#repository-structure)
14. [Key Architectural Decisions](#key-architectural-decisions)

---

## System Overview

Question Randomizer is an interview preparation application that helps users practice interview questions. The system consists of three independent services:

1. **Angular Frontend** - User interface (existing monorepo)
2. **C# Backend API** - Main API for CRUD operations and orchestration (future)
3. **TypeScript Agent Service** - AI-powered autonomous task execution (future)

### Core Features

- User authentication and authorization (Firebase Auth)
- Question and category management
- Question randomization for practice
- Interview mode
- **AI Chat Assistant** - Conversational interface for data management and automation
- Settings management

### AI Agent Capabilities

The AI agent can autonomously:
- Categorize uncategorized questions based on content analysis
- Find and fix data quality issues
- Suggest new categories based on question patterns
- Merge duplicate questions
- Analyze question difficulty and suggest improvements
- Execute complex multi-step data operations via natural language

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Internet / Users                         │
└────────────────────────────┬────────────────────────────────┘
                             ↓
                   ┌─────────────────────┐
                   │  Angular Frontend   │
                   │   (Port 4200)       │
                   │  - UI/UX            │
                   │  - User interaction │
                   └──────────┬──────────┘
                              ↓ HTTPS
                              ↓ (Single entry point)
                   ┌──────────────────────┐
                   │   C# Backend API     │
                   │   (Port 3001)        │
                   │  - Authentication    │
                   │  - CRUD operations   │
                   │  - Orchestration     │
                   │  - Business logic    │
                   └──────┬──────────┬────┘
                          ↓          ↓
                          ↓          ↓ HTTP (internal)
                   ┌──────┴──┐   ┌──┴─────────────────┐
                   │         │   │ Agent Service      │
                   │         │   │ (Port 3002)        │
                   │         │   │ - Claude SDK       │
                   │         │   │ - Agent Tools      │
                   │         │   │ - AI Tasks         │
                   │         │   └────────┬───────────┘
                   │         │            ↓
                   │         │      (via agent tools)
                   │         │            ↓
                   ↓         ↓            ↓
            ┌──────────────────────────────┐
            │    Firebase Firestore        │
            │    (Database)                │
            │  - Users                     │
            │  - Questions                 │
            │  - Categories                │
            │  - Qualifications            │
            │  - Conversations             │
            │  - Messages                  │
            └──────────────────────────────┘
```

---

## Components

### 1. Angular Frontend

**Location:** `worse-and-pricier/` monorepo
**Technology:** Angular 20, TypeScript
**State Management:** @ngrx/signals (SignalStore)
**Styling:** SCSS, Design System

**Responsibilities:**
- User interface rendering
- User interaction handling
- Client-side routing
- Form validation
- State management (UI state only)
- **Single API client** - communicates ONLY with C# Backend

**Key Libraries:**
- Angular 20 (standalone components)
- @ngrx/signals (state management)
- @jsverse/transloco (i18n - English & Polish)
- @angular/fire (Firebase Auth client)
- Design system components

**Does NOT:**
- ❌ Talk directly to Agent Service
- ❌ Have direct Firestore write access for chat data
- ❌ Know about internal service architecture

---

### 2. C# Backend API

**Location:** `question-randomizer-backend/` (future repository)
**Technology:** ASP.NET Core, C#
**Port:** 3001

**Responsibilities:**
- **Authentication & Authorization** - Verify Firebase tokens, enforce permissions
- **CRUD Operations** - Questions, categories, qualifications, conversations, messages
- **Business Logic** - Validation, data transformation, business rules
- **Orchestration** - Coordinate between Firestore and Agent Service
- **API Gateway** - Single entry point for frontend

**Endpoints:**

```
Authentication:
  POST   /api/auth/verify           - Verify Firebase token

Questions:
  GET    /api/questions             - List all questions
  GET    /api/questions/:id         - Get question by ID
  POST   /api/questions             - Create question
  PUT    /api/questions/:id         - Update question
  DELETE /api/questions/:id         - Delete question

Categories:
  GET    /api/categories            - List all categories
  POST   /api/categories            - Create category
  PUT    /api/categories/:id        - Update category
  DELETE /api/categories/:id        - Delete category

Agent Tasks:
  POST   /api/agent/tasks           - Execute AI agent task
  GET    /api/agent/tasks/:id       - Get task status/result

Conversations:
  GET    /api/conversations         - List user's conversations
  POST   /api/conversations         - Create conversation
  GET    /api/conversations/:id/messages  - Get messages
  DELETE /api/conversations/:id     - Delete conversation
```

**Key Services:**
- `IFirestoreService` - Firestore operations
- `IAgentService` - HTTP client to Agent Service
- `IAuthService` - Firebase token validation
- `IQuestionService` - Business logic for questions
- `ICategoryService` - Business logic for categories

**Database Access:**
- Firebase Admin SDK for .NET
- Direct Firestore read/write operations
- Transaction support for data consistency

---

### 3. TypeScript Agent Service

**Location:** `question-randomizer-agent/` (future repository)
**Technology:** Node.js, TypeScript, Express.js
**Port:** 3002 (internal network)

**Responsibilities:**
- **AI Task Execution** - Execute autonomous multi-step AI tasks
- **Claude Agent SDK Integration** - Use Claude's agent capabilities
- **Agent Tool Implementation** - Custom tools for Firestore access
- **Autonomous Decision Making** - Agent decides how to complete tasks

**Architecture Pattern:** Ephemeral (task-based)
- New agent instance per request
- Stateless execution
- State loaded from Firestore if needed

**Endpoints:**

```
Agent Execution:
  POST   /agent/task               - Execute an agent task
    Request: { task: string, userId: string }
    Response: Streaming JSON (agent messages)
```

**Agent Tools (Firestore Access):**

```typescript
Tools the agent can use autonomously:

Data Retrieval:
  - get_questions              - Fetch questions with optional filters
  - get_question_by_id         - Get specific question
  - get_categories             - Fetch all categories
  - get_uncategorized_questions - Get questions without category
  - search_questions           - Full-text search questions

Data Modification:
  - create_question            - Create new question
  - update_question            - Update question fields
  - delete_question            - Delete question
  - update_question_category   - Assign category to question
  - create_category            - Create new category

Data Analysis:
  - find_duplicate_questions   - Find potential duplicates
  - analyze_question_difficulty - Analyze question complexity
  - suggest_categories         - Suggest category assignments
```

**Key Dependencies:**
- `@anthropic-ai/claude-agent-sdk` - AI agent framework
- `express` - HTTP server
- `firebase-admin` - Firestore access
- `zod` - Schema validation for agent tools

**Example Task Execution:**

User request: *"Categorize all uncategorized questions"*

Agent autonomous workflow:
1. Calls `get_uncategorized_questions()` → Gets 15 questions
2. Calls `get_categories()` → Gets available categories
3. For each question:
   - Analyzes question content
   - Decides appropriate category
   - Calls `update_question_category(questionId, categoryId)`
4. Returns summary: "Categorized 15 questions: 8 JavaScript, 4 OOP, 3 Python"

---

## Technology Stack

### Frontend
- **Framework:** Angular 20
- **Language:** TypeScript
- **State Management:** @ngrx/signals
- **Styling:** SCSS
- **i18n:** @jsverse/transloco
- **Auth:** @angular/fire (Firebase Auth client)
- **Testing:** Jest, Playwright
- **Build Tool:** Nx

### Backend API
- **Framework:** ASP.NET Core
- **Language:** C#
- **Database Client:** Firebase Admin SDK for .NET
- **Auth:** Firebase Admin (token verification)
- **Testing:** xUnit / NUnit
- **API Documentation:** Swagger/OpenAPI

### Agent Service
- **Runtime:** Node.js
- **Language:** TypeScript
- **Framework:** Express.js
- **AI SDK:** @anthropic-ai/claude-agent-sdk
- **Database Client:** Firebase Admin SDK (Node.js)
- **Testing:** Jest
- **Build Tool:** TypeScript Compiler / esbuild

### Shared Infrastructure
- **Database:** Firebase Firestore
- **Authentication:** Firebase Authentication
- **Storage:** Firebase Storage (if needed for attachments)
- **Hosting:** TBD (Cloud Run, App Engine, or traditional VMs)

---

## Database Schema

Firebase Firestore is a NoSQL document database. Below is the complete schema for all collections.

### Collections Overview

```
Firestore Database
├── users/                          (User profiles)
├── questions/                      (Interview questions)
├── categories/                     (Question categories)
├── qualifications/                 (Question qualifications/job roles)
├── randomizations/                 (Randomization session metadata)
├── selectedCategories/             (Categories selected for randomization session)
├── usedQuestions/                  (Questions already shown in session)
├── postponedQuestions/             (Questions postponed for later in session)
├── conversations/                  (AI chat conversations)
└── messages/                       (Chat messages)
```

---

### 1. `users` Collection

**Purpose:** Store user profile information

**Document ID:** Firebase Auth UID

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `uid` | `string` | Yes | Firebase Auth user ID (document ID) |
| `email` | `string` | Yes | User email address |
| `created` | `Timestamp` | Yes | Account creation timestamp |

**Example Document:**

```json
{
  "uid": "user123",
  "email": "user@example.com",
  "created": "2025-11-22T10:00:00.000Z"
}
```

**Indexes Required:**

```
- None (direct document access by uid)
```

**Source:** `auth.repository.ts:109`

---

### 2. `questions` Collection

**Purpose:** Store interview questions created by users

**Document ID:** Auto-generated by Firestore

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Document ID (auto-generated) |
| `question` | `string` | Yes | Question text (English) |
| `answer` | `string` | Yes | Answer text (English) |
| `answerPl` | `string` | Yes | Answer text (Polish) |
| `categoryId` | `string \| null` | No | Reference to category document ID |
| `categoryName` | `string` | No | Denormalized category name |
| `qualificationId` | `string \| null` | No | Reference to qualification document ID |
| `qualificationName` | `string` | No | Denormalized qualification name |
| `isActive` | `boolean` | Yes | Whether question is active |
| `userId` | `string` | Yes | User ID from Firebase Auth |
| `tags` | `string[] \| undefined` | No | Array of tags for categorization |

**Example Document:**

```json
{
  "id": "q1abc123",
  "question": "What is a closure in JavaScript?",
  "answer": "A closure is a function that has access to variables in its outer scope...",
  "answerPl": "Domknięcie to funkcja, która ma dostęp do zmiennych w swoim zewnętrznym zakresie...",
  "categoryId": "cat_js_001",
  "categoryName": "JavaScript",
  "qualificationId": "qual_frontend_001",
  "qualificationName": "Frontend Developer",
  "isActive": true,
  "userId": "user123",
  "tags": ["javascript", "closures", "scope"]
}
```

**Indexes Required:**

```
- userId (ascending)
- userId (ascending), categoryId (ascending)
- userId (ascending), qualificationId (ascending)
- userId (ascending), isActive (ascending)
```

**Source:** `question.repository.ts:27`

---

### 3. `categories` Collection

**Purpose:** Categorize questions by topic (e.g., JavaScript, Python, Algorithms)

**Document ID:** Auto-generated by Firestore

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Document ID (auto-generated) |
| `name` | `string` | Yes | Category name |
| `userId` | `string` | Yes | User ID from Firebase Auth |

**Example Document:**

```json
{
  "id": "cat_js_001",
  "name": "JavaScript",
  "userId": "user123"
}
```

**Indexes Required:**

```
- userId (ascending)
```

**Source:** `category.repository.ts:23`

---

### 4. `qualifications` Collection

**Purpose:** Group questions by job qualification (e.g., Frontend Developer, Backend Engineer)

**Document ID:** Auto-generated by Firestore

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Document ID (auto-generated) |
| `name` | `string` | Yes | Qualification name |
| `userId` | `string` | Yes | User ID from Firebase Auth |

**Example Document:**

```json
{
  "id": "qual_frontend_001",
  "name": "Frontend Developer",
  "userId": "user123"
}
```

**Indexes Required:**

```
- userId (ascending)
```

**Source:** `qualification.repository.ts:23`

---

### 5. `randomizations` Collection

**Purpose:** Store randomization session metadata per user

**Document ID:** Auto-generated by Firestore

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Document ID (auto-generated) |
| `userId` | `string` | Yes | User ID from Firebase Auth |
| `showAnswer` | `boolean` | Yes | Whether answer is currently visible |
| `status` | `number` | Yes | Session status: 1=Ongoing, 2=Finished |
| `currentQuestionId` | `string \| null` | No | Reference to current question ID |
| `created` | `Timestamp` | Yes | Session creation timestamp |

**Example Document:**

```json
{
  "id": "rand_abc123",
  "userId": "user123",
  "showAnswer": false,
  "status": 1,
  "currentQuestionId": "q1abc123",
  "created": "2025-11-22T10:30:00.000Z"
}
```

**Indexes Required:**

```
- userId (ascending)
```

**Source:** `randomization.repository.ts:26`

**Note:** One active randomization per user (query by userId)

---

### 6. `selectedCategories` Collection

**Purpose:** Track which categories are selected for the current randomization session

**Document ID:** Auto-generated by Firestore

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Document ID (auto-generated) |
| `randomizationId` | `string` | Yes | Reference to randomization document ID |
| `categoryId` | `string` | Yes | Reference to category document ID |
| `created` | `Timestamp` | Yes | Selection timestamp |

**Example Document:**

```json
{
  "id": "sel_cat_001",
  "randomizationId": "rand_abc123",
  "categoryId": "cat_js_001",
  "created": "2025-11-22T10:30:00.000Z"
}
```

**Indexes Required:**

```
- randomizationId (ascending)
- randomizationId (ascending), categoryId (ascending)
- categoryId (ascending), randomizationId (ascending)
```

**Source:** `selected-category-list.repository.ts:21`

---

### 7. `usedQuestions` Collection

**Purpose:** Track questions that have been shown in the current randomization session

**Document ID:** Auto-generated by Firestore

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Document ID (auto-generated) |
| `randomizationId` | `string` | Yes | Reference to randomization document ID |
| `questionId` | `string` | Yes | Reference to question document ID |
| `categoryId` | `string \| null` | No | Reference to category document ID |
| `created` | `Timestamp` | Yes | Timestamp when question was used |

**Example Document:**

```json
{
  "id": "used_q_001",
  "randomizationId": "rand_abc123",
  "questionId": "q1abc123",
  "categoryId": "cat_js_001",
  "created": "2025-11-22T10:30:00.000Z"
}
```

**Indexes Required:**

```
- randomizationId (ascending), created (ascending)
- questionId (ascending), randomizationId (ascending)
- randomizationId (ascending), categoryId (ascending)
```

**Source:** `used-question-list.repository.ts:24`

---

### 8. `postponedQuestions` Collection

**Purpose:** Track questions that the user wants to see later in the session

**Document ID:** Auto-generated by Firestore

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Document ID (auto-generated) |
| `randomizationId` | `string` | Yes | Reference to randomization document ID |
| `questionId` | `string` | Yes | Reference to question document ID |
| `categoryId` | `string \| null` | No | Reference to category document ID |
| `created` | `Timestamp` | Yes | Timestamp when question was postponed |

**Example Document:**

```json
{
  "id": "postponed_q_001",
  "randomizationId": "rand_abc123",
  "questionId": "q5xyz789",
  "categoryId": "cat_python_001",
  "created": "2025-11-22T10:35:00.000Z"
}
```

**Indexes Required:**

```
- randomizationId (ascending), created (ascending)
- questionId (ascending), randomizationId (ascending)
- randomizationId (ascending), categoryId (ascending)
```

**Source:** `postponed-question-list.repository.ts:25-28`

---

### 9. `conversations` Collection

**Purpose:** Store AI chat conversation metadata

**Document ID:** Auto-generated by Firestore

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Document ID (auto-generated) |
| `userId` | `string` | Yes | User ID from Firebase Auth |
| `title` | `string` | Yes | Conversation title/summary |
| `createdAt` | `Timestamp` | Yes | Conversation creation timestamp |
| `updatedAt` | `Timestamp` | Yes | Last update timestamp |

**Example Document:**

```json
{
  "id": "conv_abc123",
  "userId": "user123",
  "title": "Categorizing JavaScript questions",
  "createdAt": "2025-11-22T10:30:00.000Z",
  "updatedAt": "2025-11-22T10:45:00.000Z"
}
```

**Indexes Required:**

```
- userId (ascending), updatedAt (descending)
```

**Source:** `chat.repository.ts:25`

---

### 10. `messages` Collection

**Purpose:** Store individual chat messages (user and AI assistant)

**Document ID:** Auto-generated by Firestore

**Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Document ID (auto-generated) |
| `conversationId` | `string` | Yes | Reference to conversation document ID |
| `role` | `string` | Yes | Message sender: `"user"` or `"assistant"` |
| `content` | `string` | Yes | Message text content |
| `timestamp` | `Timestamp` | Yes | Message creation timestamp |

**Example Documents:**

```json
// User message
{
  "id": "msg_user_001",
  "conversationId": "conv_abc123",
  "role": "user",
  "content": "Can you categorize all my uncategorized questions?",
  "timestamp": "2025-11-22T10:30:00.000Z"
}

// Assistant message
{
  "id": "msg_ai_001",
  "conversationId": "conv_abc123",
  "role": "assistant",
  "content": "I've categorized 15 questions. 8 were assigned to JavaScript, 4 to OOP, and 3 to Python.",
  "timestamp": "2025-11-22T10:30:15.000Z"
}
```

**Indexes Required:**

```
- conversationId (ascending), timestamp (ascending)
```

**Source:** `chat.repository.ts:26`

---

### Data Relationships Diagram

```
users
    ↓ (1:N)
    ├─→ questions
    │       ↓ (N:1)
    │       ├─→ categories
    │       └─→ qualifications
    │
    ├─→ categories
    │
    ├─→ qualifications
    │
    ├─→ randomizations
    │       ↓ (1:N)
    │       ├─→ selectedCategories → categories
    │       ├─→ usedQuestions → questions
    │       └─→ postponedQuestions → questions
    │
    └─→ conversations
            ↓ (1:N)
            └─→ messages
```

**Key Relationships:**

1. **User → Questions/Categories/Qualifications** (1:N)
   - Each user owns multiple questions, categories, and qualifications

2. **Questions → Categories/Qualifications** (N:1)
   - Questions reference categories and qualifications (optional)
   - Denormalized: `categoryName` and `qualificationName` stored in questions

3. **Randomization Session** (Per User)
   - One active randomization per user
   - `selectedCategories`: Which categories to include in session
   - `usedQuestions`: Questions already shown
   - `postponedQuestions`: Questions saved for later
   - All three collections reference `randomizationId`

4. **Conversations → Messages** (1:N)
   - Each conversation has multiple messages
   - Messages ordered by timestamp

---

### Denormalization Strategy

The schema uses **selective denormalization** for performance:

**Why denormalize `categoryName` and `qualificationName` in `questions`?**
- ✅ Avoid joins when displaying question lists
- ✅ Faster queries (no need to fetch category/qualification separately)
- ❌ Trade-off: Must update questions when category/qualification name changes

**When category name changes:**
```typescript
// Update category
await updateCategory(categoryId, newName);

// Update all questions with this category (batch update)
const questions = await getQuestionsByCategoryId(categoryId);
for (const question of questions) {
  await updateQuestion(question.id, { categoryName: newName });
}
```

---

### Security Rules

**Firestore Security Rules enforce data isolation:**

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {

    // Helper function to check randomization ownership
    function ownsRandomization(randomizationId) {
      return request.auth != null
        && exists(/databases/$(database)/documents/randomizations/$(randomizationId))
        && get(/databases/$(database)/documents/randomizations/$(randomizationId)).data.userId == request.auth.uid;
    }

    // Helper function to check conversation ownership
    function ownsConversation(conversationId) {
      return request.auth != null
        && exists(/databases/$(database)/documents/conversations/$(conversationId))
        && get(/databases/$(database)/documents/conversations/$(conversationId)).data.userId == request.auth.uid;
    }

    // Users: can only read/write their own profile
    match /users/{userId} {
      allow read, write: if request.auth != null
        && request.auth.uid == userId;
    }

    // Questions: users can only access their own
    match /questions/{questionId} {
      allow read, write: if request.auth != null
        && request.auth.uid == resource.data.userId;
      allow create: if request.auth != null
        && request.auth.uid == request.resource.data.userId;
    }

    // Categories: users can only access their own
    match /categories/{categoryId} {
      allow read, write: if request.auth != null
        && request.auth.uid == resource.data.userId;
      allow create: if request.auth != null
        && request.auth.uid == request.resource.data.userId;
    }

    // Qualifications: users can only access their own
    match /qualifications/{qualificationId} {
      allow read, write: if request.auth != null
        && request.auth.uid == resource.data.userId;
      allow create: if request.auth != null
        && request.auth.uid == request.resource.data.userId;
    }

    // Randomizations: users can only access their own
    match /randomizations/{randomizationId} {
      allow read, write: if request.auth != null
        && request.auth.uid == resource.data.userId;
      allow create: if request.auth != null
        && request.auth.uid == request.resource.data.userId;
    }

    // SelectedCategories: can only access if owns the randomization
    match /selectedCategories/{selectedCategoryId} {
      allow read, write: if ownsRandomization(resource.data.randomizationId);
      allow create: if ownsRandomization(request.resource.data.randomizationId);
    }

    // UsedQuestions: can only access if owns the randomization
    match /usedQuestions/{usedQuestionId} {
      allow read, write: if ownsRandomization(resource.data.randomizationId);
      allow create: if ownsRandomization(request.resource.data.randomizationId);
    }

    // PostponedQuestions: can only access if owns the randomization
    match /postponedQuestions/{postponedQuestionId} {
      allow read, write: if ownsRandomization(resource.data.randomizationId);
      allow create: if ownsRandomization(request.resource.data.randomizationId);
    }

    // Conversations: users can only access their own
    match /conversations/{conversationId} {
      allow read, write: if request.auth != null
        && request.auth.uid == resource.data.userId;
      allow create: if request.auth != null
        && request.auth.uid == request.resource.data.userId;
    }

    // Messages: can only access if owns the conversation
    match /messages/{messageId} {
      allow read, write: if ownsConversation(resource.data.conversationId);
      allow create: if ownsConversation(request.resource.data.conversationId);
    }
  }
}
```

---

### Migration Considerations

**Current State (Frontend):**
- Frontend has direct Firestore write access via `ChatRepository`

**Future State (Backend API + Agent Service):**
- C# Backend handles all CRUD operations
- Agent Service has Firestore access via MCP tools
- Frontend only calls C# Backend API

**Migration Steps:**
1. Deploy C# Backend with Firestore access
2. Deploy Agent Service with MCP tools
3. Update frontend to call C# Backend instead of direct Firestore
4. Remove `ChatRepository` from frontend
5. Update Firestore security rules to restrict direct frontend writes

**No schema changes required** - the database structure remains the same.

---

## Data Flow

### 1. Traditional CRUD Operation (e.g., Get Questions)

```
User clicks "View Questions"
    ↓
Frontend: HTTP GET /api/questions
    ↓
C# Backend:
    - Verify Firebase token
    - Extract userId from token
    - Query Firestore: questions WHERE userId = {userId}
    - Return questions
    ↓
Frontend: Display questions in UI
```

**Timing:** ~50-100ms
**Cost:** Minimal (Firestore read)

---

### 2. AI Agent Task (e.g., Auto-categorize Questions)

```
User clicks "Auto-categorize" button
    ↓
Frontend: HTTP POST /api/agent/tasks
    Body: { task: "Categorize all uncategorized questions" }
    ↓
C# Backend:
    - Verify Firebase token
    - Extract userId from token
    - Log task initiation in Firestore
    - Forward to Agent Service: HTTP POST http://agent-service:3002/agent/task
      Body: { task: "...", userId: "..." }
    ↓
Agent Service:
    - Initialize Claude Agent SDK
    - Provide agent tools (Firestore access)
    - Execute agent loop:
        1. Agent thinks: "I need uncategorized questions"
        2. Agent calls: get_uncategorized_questions()
        3. Tool executes: Firestore query
        4. Agent receives: [15 questions]
        5. Agent thinks: "I need categories"
        6. Agent calls: get_categories()
        7. Tool executes: Firestore query
        8. Agent receives: [5 categories]
        9. Agent thinks: "Question X fits category Y"
        10. Agent calls: update_question_category(X, Y)
        11. Tool executes: Firestore update
        12. ... repeats for all questions ...
    - Return result summary
    ↓
C# Backend:
    - Receive agent result
    - Log task completion in Firestore
    - Return result to frontend
    ↓
Frontend: Display success message with summary
```

**Timing:** ~5-30 seconds (depends on task complexity)
**Cost:** Firestore reads/writes + Claude API calls

---

### 3. Chat Conversation Flow

```
User sends message in AI Chat
    ↓
Frontend: HTTP POST /api/conversations/{id}/messages
    Body: { message: "Show me all JavaScript questions" }
    ↓
C# Backend:
    - Verify Firebase token
    - Save user message to Firestore
    - Forward to Agent Service with conversation context
    ↓
Agent Service:
    - Load conversation history from Firestore (via agent tool)
    - Execute agent with context
    - Agent may use tools like get_questions(category: "JavaScript")
    - Generate response
    - Return response
    ↓
C# Backend:
    - Save agent response to Firestore
    - Update conversation timestamp
    - Return both messages to frontend
    ↓
Frontend: Display messages in chat UI
```

---

## API Design

### C# Backend API Conventions

**Base URL:** `http://localhost:3001/api` (dev) or `https://api.questionrandomizer.com/api` (prod)

**Authentication:**
```
All requests require Firebase ID token:
Authorization: Bearer {firebase-id-token}
```

**Response Format:**
```json
Success (200 OK):
{
  "data": { ... },
  "timestamp": "2025-11-22T10:30:00Z"
}

Error (400/401/500):
{
  "error": {
    "code": "INVALID_REQUEST",
    "message": "User-friendly error message",
    "details": { ... }
  },
  "timestamp": "2025-11-22T10:30:00Z"
}
```

**Pagination:**
```
Query params: ?page=1&limit=20&sortBy=createdAt&order=desc

Response:
{
  "data": [...],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 150,
    "totalPages": 8
  }
}
```

---

## Agent Service Design

### Agent Configuration

```typescript
// Agent options for different task types
const agentOptions = {
  maxTurns: 20,                    // Prevent infinite loops
  model: 'claude-sonnet-4.5',      // Latest Claude model
  tools: [
    getQuestions,
    createQuestion,
    updateQuestion,
    getCategories,
    // ... other tools
  ]
};
```

### Agent Tool Implementation Example

```typescript
import { tool } from '@anthropic-ai/claude-agent-sdk';
import { z } from 'zod';

const getQuestionsSchema = z.object({
  categoryId: z.string().optional(),
  limit: z.number().max(100).default(50),
  search: z.string().optional()
});

tool(
  'get_questions',
  'Fetch questions with optional filters',
  getQuestionsSchema,
  async (args) => {
    let query = db.collection('questions')
      .where('userId', '==', currentUserId);

    if (args.categoryId) {
      query = query.where('categoryId', '==', args.categoryId);
    }

    if (args.search) {
      // Implement search logic
    }

    const snapshot = await query.limit(args.limit).get();
    const questions = snapshot.docs.map(doc => ({
      id: doc.id,
      ...doc.data()
    }));

    return {
      content: [{
        type: 'text',
        text: JSON.stringify(questions, null, 2)
      }]
    };
  }
)
```

### Error Handling

```typescript
app.post('/agent/task', async (req, res) => {
  try {
    const { task, userId } = req.body;

    // Set timeout for long-running tasks
    const timeout = setTimeout(() => {
      res.status(408).json({ error: 'Task timeout' });
    }, 60000); // 60 seconds

    for await (const msg of query({
      prompt: task,
      options: agentOptions
    })) {
      res.write(JSON.stringify(msg) + '\n');
    }

    clearTimeout(timeout);
    res.end();

  } catch (error) {
    if (error instanceof CLINotFoundError) {
      res.status(500).json({ error: 'Claude CLI not installed' });
    } else if (error instanceof ProcessError) {
      res.status(500).json({ error: 'Agent execution failed' });
    } else {
      res.status(500).json({ error: error.message });
    }
  }
});
```

---

## Database Access Patterns

### C# Backend - Direct Firestore Access

**Purpose:** Fast, predictable CRUD operations

**Pattern:**
```csharp
public class FirestoreService : IFirestoreService
{
    private readonly FirestoreDb _db;

    public async Task<List<Question>> GetQuestions(string userId)
    {
        var query = _db.Collection("questions")
            .WhereEqualTo("userId", userId);

        var snapshot = await query.GetSnapshotAsync();

        return snapshot.Documents
            .Select(doc => doc.ConvertTo<Question>())
            .ToList();
    }

    public async Task<string> CreateQuestion(Question question)
    {
        var docRef = await _db.Collection("questions").AddAsync(question);
        return docRef.Id;
    }
}
```

**When to use:**
- User requests specific data
- Predictable queries
- Need fast response times
- Simple CRUD operations

---

### Agent Service - Tool-Based Firestore Access

**Purpose:** Dynamic, autonomous data operations

**Pattern:**
```typescript
// Define tools that the agent can use
const getQuestions = tool(
  'get_questions',
  'Fetch questions from Firestore',
  schema,
  async (args) => {
    // Agent decided to call this with specific args
    const questions = await db.collection('questions')
      .where('userId', '==', args.userId)
      .get();

    return { content: [{ type: 'text', text: JSON.stringify(questions) }] };
  }
);

// Agent autonomously decides:
// - Which tools to call
// - In what order
// - With what parameters
// - How many iterations
```

**When to use:**
- Complex multi-step tasks
- Data analysis and cleanup
- Pattern recognition across data
- Tasks requiring reasoning and decision-making

---

### Data Consistency

**Strategy:** Optimistic concurrency with Firestore transactions

```csharp
// C# Backend - Transaction example
public async Task UpdateQuestionWithCategory(string questionId, string categoryId)
{
    var questionRef = _db.Collection("questions").Document(questionId);

    await _db.RunTransactionAsync(async transaction =>
    {
        var snapshot = await transaction.GetSnapshotAsync(questionRef);

        if (!snapshot.Exists)
            throw new NotFoundException("Question not found");

        transaction.Update(questionRef, new Dictionary<string, object>
        {
            { "categoryId", categoryId },
            { "updatedAt", FieldValue.ServerTimestamp }
        });
    });
}
```

**Conflict Resolution:**
- Use Firestore transactions for critical updates
- Last-write-wins for non-critical updates
- Agent tasks are idempotent where possible

---

## Security Architecture

### 1. Authentication Flow

```
User logs in with email/password
    ↓
Firebase Auth generates ID token
    ↓
Frontend stores token (memory/session storage)
    ↓
Frontend sends token with every API request
    ↓
C# Backend verifies token with Firebase Admin SDK
    ↓
Backend extracts userId and claims from token
    ↓
Backend authorizes request
```

### 2. Authorization Model

**Firestore Security Rules:**
```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {

    // Users can only access their own data
    match /questions/{questionId} {
      allow read, write: if request.auth != null
        && request.auth.uid == resource.data.userId;
    }

    match /categories/{categoryId} {
      allow read, write: if request.auth != null
        && request.auth.uid == resource.data.userId;
    }

    match /conversations/{conversationId} {
      allow read, write: if request.auth != null
        && request.auth.uid == resource.data.userId;
    }

    match /messages/{messageId} {
      allow read, write: if request.auth != null
        && exists(/databases/$(database)/documents/conversations/$(resource.data.conversationId))
        && get(/databases/$(database)/documents/conversations/$(resource.data.conversationId)).data.userId == request.auth.uid;
    }
  }
}
```

**C# Backend Authorization:**
```csharp
[Authorize] // Require authenticated user
[HttpGet("api/questions")]
public async Task<IActionResult> GetQuestions()
{
    var userId = User.Claims.First(c => c.Type == "user_id").Value;

    // Only return questions owned by this user
    var questions = await _questionService.GetQuestions(userId);

    return Ok(questions);
}
```

### 3. Network Security

**Production Setup:**
```
Internet
    ↓ HTTPS only
C# Backend (public endpoint)
    ↓ HTTP (internal network, private subnet)
Agent Service (NOT exposed to internet)
```

**Agent Service Security:**
- Listen on internal network only: `0.0.0.0:3002`
- No CORS needed (only C# Backend calls it)
- No public IP address
- Firewall rules: Only C# Backend can reach Agent Service
- Optional: mTLS between C# Backend and Agent Service

### 4. API Key Management

**Environment Variables:**

```bash
# C# Backend
FIREBASE_PROJECT_ID=your-project-id
FIREBASE_PRIVATE_KEY=...
AGENT_SERVICE_URL=http://agent-service:3002
AGENT_SERVICE_API_KEY=secret-key-for-agent-communication

# Agent Service
ANTHROPIC_API_KEY=sk-ant-...
FIREBASE_PROJECT_ID=your-project-id
FIREBASE_PRIVATE_KEY=...
```

**Never commit:**
- Firebase service account keys
- Anthropic API keys
- Any credentials

Use secret management:
- Google Cloud Secret Manager
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault

---

## Scalability Considerations

### Horizontal Scaling

**C# Backend:**
```
Load Balancer
    ├─→ Backend Instance 1
    ├─→ Backend Instance 2
    ├─→ Backend Instance 3
    └─→ Backend Instance N

Scale based on:
- Request rate (requests/second)
- CPU usage
- Memory usage
```

**Agent Service:**
```
Agent Load Balancer
    ├─→ Agent Instance 1 (handles 2-3 concurrent tasks)
    └─→ Agent Instance 2 (handles 2-3 concurrent tasks)

Scale based on:
- Active agent tasks
- Queue depth
- Average task duration
```

### Performance Optimization

**C# Backend Caching:**
```csharp
// Cache categories (rarely change)
private readonly IMemoryCache _cache;

public async Task<List<Category>> GetCategories(string userId)
{
    var cacheKey = $"categories:{userId}";

    if (_cache.TryGetValue(cacheKey, out List<Category> categories))
        return categories;

    categories = await _firestore.GetCategories(userId);

    _cache.Set(cacheKey, categories, TimeSpan.FromMinutes(5));

    return categories;
}
```

**Firestore Optimization:**
- Create composite indexes for common queries
- Use pagination for large result sets
- Implement field masking (only fetch needed fields)
- Use Firestore streaming for real-time updates

**Agent Service Optimization:**
- Implement task queue (e.g., Bull, RabbitMQ)
- Limit concurrent agent executions
- Cache agent responses for identical tasks
- Implement timeout mechanisms

### Database Scaling

**Firestore automatically scales:**
- Distributed across regions
- Auto-sharding
- No manual scaling needed

**Monitoring:**
- Document read/write rates
- Query performance
- Index usage
- Storage size

---

## Deployment Architecture

### Development Environment

```
Developer Machine:
├─ Frontend (localhost:4200) - ng serve
├─ C# Backend (localhost:3001) - dotnet run
└─ Agent Service (localhost:3002) - node dist/main.js

All services connect to:
- Firebase Firestore (dev project)
- Firebase Auth (dev project)
```

### Production Environment (Cloud Run - Example)

```
Cloud Load Balancer (HTTPS)
    ↓
┌────────────────────────────────────────┐
│  Cloud Run: Frontend                   │
│  - Container: nginx + Angular build    │
│  - Public access                       │
└────────────┬───────────────────────────┘
             ↓ API calls
┌────────────────────────────────────────┐
│  Cloud Run: C# Backend                 │
│  - Container: ASP.NET Core             │
│  - Public access (authenticated)       │
│  - Auto-scaling: 1-10 instances        │
└──────┬─────────────────────────────────┘
       ↓ Internal HTTP
┌────────────────────────────────────────┐
│  Cloud Run: Agent Service              │
│  - Container: Node.js + Agent SDK      │
│  - Internal access only                │
│  - Auto-scaling: 1-3 instances         │
└──────┬─────────────────────────────────┘
       ↓
┌────────────────────────────────────────┐
│  Firebase Firestore                    │
│  - Multi-region                        │
│  - Auto-scaling                        │
└────────────────────────────────────────┘
```

### Alternative: Traditional VMs

```
VM 1: Frontend (nginx)
VM 2-N: C# Backend (load balanced)
VM M: Agent Service (internal only)
```

### CI/CD Pipeline

```
GitHub Repository
    ↓ (push to main)
GitHub Actions
    ├─ Build Frontend
    ├─ Build C# Backend
    ├─ Build Agent Service
    ├─ Run Tests
    ├─ Build Docker Images
    └─ Deploy to Cloud Run
```

---

## Repository Structure

The Question Randomizer system spans three repositories:

### 1. `worse-and-pricier/` (Frontend - Existing)

```
worse-and-pricier/                      # Nx monorepo
├── apps/
│   └── question-randomizer/            # Angular app
├── libs/
│   ├── design-system/                  # UI components
│   └── question-randomizer/
│       ├── auth/                       # Auth domain
│       ├── dashboard/
│       │   ├── questions/              # Question management
│       │   ├── categories/             # Category management
│       │   ├── ai-chat/                # AI chat UI
│       │   ├── shared/                 # Shared stores (will be refactored)
│       │   └── shell/                  # Dashboard shell
│       └── shared/                     # App-wide utilities
├── CLAUDE.md                           # Frontend architecture docs
└── README.md
```

**Future Refactoring:**
- Remove `ChatRepository` (move to backend)
- Keep `AiChatApiRepository` (calls C# Backend)
- Remove direct Firestore write access for chat

---

### 2. `question-randomizer-backend/` (C# Backend - Future)

```
question-randomizer-backend/
├── src/
│   ├── QuestionRandomizer.Api/         # ASP.NET Core API
│   │   ├── Controllers/
│   │   │   ├── QuestionsController.cs
│   │   │   ├── CategoriesController.cs
│   │   │   ├── AgentController.cs
│   │   │   └── ConversationsController.cs
│   │   ├── Middleware/
│   │   │   ├── AuthenticationMiddleware.cs
│   │   │   └── ErrorHandlingMiddleware.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── QuestionRandomizer.Core/        # Domain models & interfaces
│   │   ├── Models/
│   │   │   ├── Question.cs
│   │   │   ├── Category.cs
│   │   │   └── Conversation.cs
│   │   ├── Interfaces/
│   │   │   ├── IFirestoreService.cs
│   │   │   ├── IAgentService.cs
│   │   │   └── IQuestionService.cs
│   │   └── Exceptions/
│   │
│   └── QuestionRandomizer.Infrastructure/  # External service implementations
│       ├── Services/
│       │   ├── FirestoreService.cs
│       │   ├── AgentService.cs           # HTTP client to Agent Service
│       │   └── AuthService.cs
│       └── Mappers/
│           └── FirestoreMapper.cs
│
├── tests/
│   ├── QuestionRandomizer.Api.Tests/
│   └── QuestionRandomizer.Core.Tests/
│
├── docs/
│   ├── API_DESIGN.md                   # API endpoint documentation
│   ├── AGENT_INTEGRATION.md            # How to call Agent Service
│   └── DEPLOYMENT.md                   # Deployment instructions
│
├── Dockerfile
├── .gitignore
└── README.md
```

---

### 3. `question-randomizer-agent/` (Agent Service - Future)

```
question-randomizer-agent/
├── src/
│   ├── server.ts                       # Express server
│   ├── agent/
│   │   ├── agent-config.ts             # Agent SDK configuration
│   │   └── task-executor.ts            # Task execution logic
│   │
│   ├── tools/
│   │   ├── firestore-tools.ts          # Agent tool definitions
│   │   ├── schemas/                    # Zod schemas for tools
│   │   │   ├── question-schema.ts
│   │   │   └── category-schema.ts
│   │   └── implementations/
│   │       ├── question-tools.ts
│   │       ├── category-tools.ts
│   │       └── analysis-tools.ts
│   │
│   ├── services/
│   │   └── firestore.service.ts        # Firestore client
│   │
│   └── middleware/
│       ├── error-handler.ts
│       └── timeout.ts
│
├── tests/
│   ├── tools/
│   └── agent/
│
├── docs/
│   ├── AGENT_TOOLS.md                  # Tool documentation
│   ├── TASK_EXAMPLES.md                # Example agent tasks
│   └── TROUBLESHOOTING.md
│
├── package.json
├── tsconfig.json
├── Dockerfile
├── .env.example
└── README.md
```

---

## Key Architectural Decisions

### Decision 1: Microservices over Monolith

**Context:** Should we build one service or separate services?

**Decision:** Separate services (C# Backend + TypeScript Agent Service)

**Rationale:**
- **Separation of concerns** - CRUD operations vs AI tasks are fundamentally different
- **Independent scaling** - Scale API and Agent independently based on load
- **Technology fit** - C# for API (familiar), TypeScript for Agent (SDK requirement)
- **Deployment flexibility** - Deploy, update, rollback services independently
- **Team autonomy** - Different teams can work on different services

**Trade-offs:**
- ✅ Better scalability
- ✅ Clear boundaries
- ✅ Technology flexibility
- ❌ More complex deployment
- ❌ Inter-service communication overhead

---

### Decision 2: Frontend talks only to C# Backend

**Context:** Should frontend call both C# Backend and Agent Service?

**Decision:** Frontend calls ONLY C# Backend, which orchestrates Agent Service calls

**Rationale:**
- **Single entry point** - Simpler frontend code
- **Security** - Agent Service not exposed to internet
- **Orchestration** - Backend can coordinate multiple services
- **Flexibility** - Can change backend architecture without frontend changes
- **Better encapsulation** - Frontend doesn't know about internal services

**Trade-offs:**
- ✅ Simpler frontend
- ✅ Better security
- ✅ Easier to evolve backend
- ❌ Extra network hop (backend → agent)
- ❌ Backend becomes orchestrator (more responsibility)

---

### Decision 3: Agent needs direct Firestore access

**Context:** Should Agent Service access Firestore directly or ask C# Backend?

**Decision:** Agent Service has direct Firestore access via agent tools

**Rationale:**
- **True autonomy** - Agent can make decisions and take actions independently
- **Performance** - No roundtrip to C# Backend for every data operation
- **Agent capability** - Agent can execute complex multi-step workflows
- **Simpler architecture** - Agent directly uses tools to access data, not through API calls

**Trade-offs:**
- ✅ Real agent autonomy
- ✅ Better performance for complex tasks
- ✅ More powerful AI capabilities
- ❌ Two services writing to same database (requires coordination)
- ❌ More complex security (two services need Firestore access)

---

### Decision 4: C# for Backend, TypeScript for Agent

**Context:** What technology to use for each service?

**Decision:**
- C# ASP.NET Core for Backend API
- TypeScript/Node.js for Agent Service

**Rationale:**
- **C# Backend**:
  - Team knows C#
  - Strong typing
  - Good Firebase Admin SDK
  - Mature ecosystem for APIs

- **TypeScript Agent**:
  - Claude Agent SDK is TypeScript/Python
  - Node.js has excellent Firebase support
  - Same language as frontend (can share types)
  - Good for async operations

**Trade-offs:**
- ✅ Best tool for each job
- ✅ Team expertise utilized
- ✅ Native SDK support
- ❌ Two languages to maintain
- ❌ Different deployment stacks

---

### Decision 5: Firestore over SQL Database

**Context:** What database to use?

**Decision:** Firebase Firestore (already in use)

**Rationale:**
- **Already integrated** - Frontend uses Firebase Auth
- **Document model** - Good fit for questions, categories (hierarchical data)
- **Real-time updates** - Can add real-time features later
- **Managed service** - No database administration needed
- **Auto-scaling** - Handles growth automatically
- **Multi-platform** - Good SDKs for .NET, Node.js, Angular

**Trade-offs:**
- ✅ No ops overhead
- ✅ Real-time capabilities
- ✅ Good SDK support
- ❌ Limited query capabilities vs SQL
- ❌ Vendor lock-in
- ❌ Can be expensive at scale

---

### Decision 6: Ephemeral Agent Pattern

**Context:** Should agent instances be long-running or created per-request?

**Decision:** Ephemeral (task-based) pattern

**Rationale:**
- **Stateless** - Each task is independent
- **Cost-effective** - Only pay for agent compute during tasks
- **Simpler** - No state management in agent service
- **Scalable** - Can spin up many instances for concurrent tasks
- **Fault tolerance** - Failed task doesn't affect others

**Trade-offs:**
- ✅ Simpler architecture
- ✅ Better cost control
- ✅ Easy to scale
- ❌ No persistent agent "memory"
- ❌ Context must be reloaded each time

**Note:** Can evolve to persistent pattern later if needed (e.g., proactive monitoring)

---

## Future Considerations

### 1. Real-time Updates

Consider adding WebSocket support for real-time updates:
- Live updates when agent completes tasks
- Real-time chat messages
- Collaborative features

### 2. Agent Task Queue

For high load, implement task queue:
```
Frontend → C# Backend → Task Queue (RabbitMQ/Bull) → Agent Workers
```

### 3. Observability

Implement monitoring and logging:
- Application Performance Monitoring (APM) - Application Insights, Datadog
- Centralized logging - ELK stack, Google Cloud Logging
- Distributed tracing - OpenTelemetry
- Agent task analytics - Track success rates, execution times

### 4. Cost Optimization

Monitor and optimize Claude API usage:
- Cache common agent responses
- Implement rate limiting
- Use cheaper models for simple tasks
- Batch similar tasks

### 5. Multi-tenancy

If supporting multiple organizations:
- Add `organizationId` to all data
- Firestore: `/organizations/{orgId}/questions/{questionId}`
- Agent tools respect organization boundaries

---

## Glossary

**Agent** - Autonomous AI system that can use tools to complete tasks
**Agent Tool** - Function that an agent can call to perform actions (e.g., `get_questions`, `update_category`)
**Ephemeral Agent** - Agent instance created per-request and destroyed after
**Orchestration** - Backend coordinating calls to multiple services
**Firestore** - NoSQL document database by Google
**SignalStore** - @ngrx/signals state management pattern
**Claude Agent SDK** - TypeScript/Python library for building AI agents with custom tools

---

## Contact & Contributions

**System Owner:** [Your name/team]
**Architecture Review:** Before making significant changes to this architecture, discuss with the team
**Documentation Updates:** Keep this document in sync with implementation

---

**Last Updated:** 2025-11-22
**Version:** 1.0
**Status:** Planned (Implementation pending)
