# Question Randomizer Backend - Implementation Guide

**Project:** Question Randomizer Backend API
**Technology:** .NET 10 (C#)
**Architecture:** Clean Architecture + CQRS + MediatR
**Database:** Firebase Firestore
**Authentication:** Firebase Authentication
**Last Updated:** 2025-11-22
**Status:** Not Started - Awaiting .NET 10 SDK Installation

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Prerequisites](#prerequisites)
3. [Architecture Decisions](#architecture-decisions)
4. [Technology Stack](#technology-stack)
5. [Project Structure](#project-structure)
6. [Implementation Phases](#implementation-phases)
7. [Detailed Implementation Steps](#detailed-implementation-steps)
8. [Code Templates and Patterns](#code-templates-and-patterns)
9. [Configuration Guide](#configuration-guide)
10. [Testing Strategy](#testing-strategy)
11. [API Endpoints Specification](#api-endpoints-specification)
12. [Firebase Integration](#firebase-integration)
13. [Agent Service Integration](#agent-service-integration)
14. [Validation Checklist](#validation-checklist)

---

## Project Overview

### Purpose
Build a .NET 10 backend API that serves as the orchestration layer for the Question Randomizer application. This API handles:
- User authentication and authorization via Firebase
- CRUD operations for questions, categories, qualifications
- Orchestration of AI agent tasks
- Conversation and message management

### Key Goals
- ✅ Modern .NET 10 best practices
- ✅ Clean, maintainable architecture
- ✅ Comprehensive test coverage (Unit, Integration, E2E)
- ✅ Production-ready with proper error handling
- ✅ High performance and scalability
- ✅ Type-safe throughout

### System Context
This backend is part of a 3-service architecture:
1. **Angular Frontend** (existing) - User interface
2. **C# Backend API** (this project) - Main API and orchestration
3. **TypeScript Agent Service** (future) - AI-powered autonomous tasks

**Frontend → Backend API → [Firestore, Agent Service]**

---

## Prerequisites

### Required Software
1. **.NET 10 SDK** (LTS - November 2025 release)
   - Download: https://dotnet.microsoft.com/download/dotnet/10.0
   - Verify: `dotnet --version` → should show 10.x.x

2. **IDE/Editor** (choose one)
   - Visual Studio 2025 (recommended for Windows)
   - JetBrains Rider
   - Visual Studio Code with C# Dev Kit

3. **Docker Desktop** (for integration tests with TestContainers)
   - Required for running Firebase Emulator in tests

4. **Git** (for version control)

### Firebase Setup Required
- Firebase project ID
- Firebase Admin SDK service account JSON key file
- Firestore database created and configured
- Authentication enabled (Email/Password provider)

### Knowledge Prerequisites
- C# and .NET fundamentals
- ASP.NET Core basics
- CQRS pattern understanding
- Dependency injection concepts
- RESTful API design

---

## Architecture Decisions

### Why Controllers over Minimal APIs?
**Decision:** Use Controllers despite Minimal APIs being newer

**Rationale:**
- CQRS/MediatR integrates more naturally with Controllers
- Clean Architecture benefits from structured approach
- Filter pipelines work better with Controllers
- Better for comprehensive testing requirements
- Team familiarity (most .NET developers know Controllers)

**Note:** Both scale equally well - Minimal APIs are "minimal" in ceremony, not capability

### Why Clean Architecture?
**Decision:** Use Clean Architecture with 4 separate projects

**Benefits:**
- Clear separation of concerns
- Dependency inversion (dependencies point inward)
- Highly testable (business logic isolated from infrastructure)
- Technology-agnostic domain layer
- Long-term maintainability

**Structure:**
```
Domain ← Application ← Infrastructure
                ↑
               API
```

### Why CQRS with MediatR?
**Decision:** Use CQRS pattern with MediatR library

**Benefits:**
- Separates read and write operations
- Simplifies complex business logic
- Easy to test handlers in isolation
- Decouples controllers from business logic
- Supports cross-cutting concerns (logging, validation)

**Pattern:**
```
Controller → MediatR → Command/Query Handler → Repository → Firestore
```

### Why Repository Pattern for Firebase?
**Decision:** Abstract Firestore behind repository interfaces

**Benefits:**
- Easier to test with mocks
- Can switch databases without changing business logic
- Encapsulates Firebase-specific code
- Aligns with Clean Architecture principles

---

## Technology Stack

### Core Framework
- **.NET 10 LTS** - Latest long-term support (3 years until Nov 2028)
- **ASP.NET Core 10** - Web framework
- **C# 14** - Latest language features

### Key Libraries (NuGet Packages)

**CQRS & Validation**
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.10.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.10.0" />
```

**Firebase**
```xml
<PackageReference Include="FirebaseAdmin" Version="3.0.1" />
<PackageReference Include="Google.Cloud.Firestore" Version="3.8.0" />
```

**Error Handling & Documentation**
```xml
<PackageReference Include="Hellang.Middleware.ProblemDetails" Version="6.5.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
```

**Observability**
```xml
<PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
```

**HTTP Client & Resilience**
```xml
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="10.0.0" />
<PackageReference Include="Polly" Version="8.4.2" />
```

**Testing**
```xml
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="FluentAssertions" Version="7.0.0" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
<PackageReference Include="Testcontainers" Version="3.10.0" />
<PackageReference Include="Bogus" Version="35.6.1" />
```

### Design Patterns Used
- **CQRS** - Command Query Responsibility Segregation
- **Mediator** - Decouples request from handler
- **Repository** - Abstracts data access
- **Options** - Type-safe configuration
- **Dependency Injection** - Loose coupling
- **Factory** - Object creation (e.g., HttpClient)

---

## Project Structure

### Solution Structure
```
question-randomizer-backend/
├── QuestionRandomizer.sln                    # Solution file
├── ARCHITECTURE.md                           # System architecture (existing)
├── CLAUDE.md                                 # This file - implementation guide
├── README.md                                 # Project README
├── .gitignore                                # Git ignore file
├── global.json                               # .NET SDK version lock
├── Directory.Build.props                     # Shared MSBuild properties
│
├── src/
│   ├── QuestionRandomizer.Api/              # 🌐 API Layer (Controllers, Middleware)
│   ├── QuestionRandomizer.Application/      # 💼 Application Layer (CQRS, Business Logic)
│   ├── QuestionRandomizer.Domain/           # 🏛️ Domain Layer (Entities, Interfaces)
│   └── QuestionRandomizer.Infrastructure/   # 🔧 Infrastructure Layer (Firebase, External Services)
│
└── tests/
    ├── QuestionRandomizer.UnitTests/        # 🧪 Unit Tests (Handlers, Validators)
    ├── QuestionRandomizer.IntegrationTests/ # 🔗 Integration Tests (API + Firebase Emulator)
    └── QuestionRandomizer.E2ETests/         # 🎭 End-to-End Tests (Full System)
```

### Detailed Project Breakdown

#### 1. QuestionRandomizer.Domain (Core)
```
QuestionRandomizer.Domain/
├── Entities/
│   ├── Question.cs                  # Question entity
│   ├── Category.cs                  # Category entity
│   ├── Qualification.cs             # Qualification entity
│   ├── Conversation.cs              # Conversation entity
│   ├── Message.cs                   # Message entity
│   ├── Randomization.cs             # Randomization session entity
│   └── User.cs                      # User entity
│
├── ValueObjects/                    # Immutable domain objects
│   └── (optional - add if needed)
│
├── Interfaces/
│   ├── IQuestionRepository.cs       # Question data access contract
│   ├── ICategoryRepository.cs       # Category data access contract
│   ├── IQualificationRepository.cs  # Qualification data access contract
│   ├── IConversationRepository.cs   # Conversation data access contract
│   ├── IMessageRepository.cs        # Message data access contract
│   └── IRandomizationRepository.cs  # Randomization data access contract
│
├── Exceptions/
│   ├── DomainException.cs           # Base domain exception
│   ├── NotFoundException.cs         # Entity not found
│   ├── ValidationException.cs       # Validation failure
│   └── UnauthorizedException.cs     # Authorization failure
│
└── Common/
    └── BaseEntity.cs                # Base class for entities (optional)
```

**Dependencies:** None (pure domain logic)

#### 2. QuestionRandomizer.Application (Business Logic)
```
QuestionRandomizer.Application/
├── Commands/
│   ├── Questions/
│   │   ├── CreateQuestion/
│   │   │   ├── CreateQuestionCommand.cs
│   │   │   ├── CreateQuestionCommandHandler.cs
│   │   │   └── CreateQuestionCommandValidator.cs
│   │   ├── UpdateQuestion/
│   │   │   ├── UpdateQuestionCommand.cs
│   │   │   ├── UpdateQuestionCommandHandler.cs
│   │   │   └── UpdateQuestionCommandValidator.cs
│   │   └── DeleteQuestion/
│   │       ├── DeleteQuestionCommand.cs
│   │       └── DeleteQuestionCommandHandler.cs
│   │
│   ├── Categories/
│   │   ├── CreateCategory/
│   │   ├── UpdateCategory/
│   │   └── DeleteCategory/
│   │
│   └── Agent/
│       └── ExecuteAgentTask/
│           ├── ExecuteAgentTaskCommand.cs
│           └── ExecuteAgentTaskCommandHandler.cs
│
├── Queries/
│   ├── Questions/
│   │   ├── GetQuestions/
│   │   │   ├── GetQuestionsQuery.cs
│   │   │   └── GetQuestionsQueryHandler.cs
│   │   └── GetQuestionById/
│   │       ├── GetQuestionByIdQuery.cs
│   │       └── GetQuestionByIdQueryHandler.cs
│   │
│   ├── Categories/
│   │   └── GetCategories/
│   │
│   └── Conversations/
│       └── GetConversationMessages/
│
├── DTOs/                            # Data Transfer Objects
│   ├── QuestionDto.cs
│   ├── CategoryDto.cs
│   ├── ConversationDto.cs
│   ├── MessageDto.cs
│   ├── CreateQuestionRequest.cs
│   ├── UpdateQuestionRequest.cs
│   └── ApiResponse.cs
│
├── Interfaces/
│   ├── IAgentService.cs             # Agent Service HTTP client interface
│   └── ICurrentUserService.cs       # Current user context interface
│
├── Mappings/
│   └── MappingProfile.cs            # AutoMapper profile (if using AutoMapper)
│
├── Behaviors/                       # MediatR pipeline behaviors
│   ├── ValidationBehavior.cs        # Validates commands/queries before handling
│   ├── LoggingBehavior.cs           # Logs all requests
│   └── PerformanceBehavior.cs       # Tracks performance metrics
│
└── DependencyInjection.cs           # Register Application services
```

**Dependencies:** Domain, MediatR, FluentValidation

#### 3. QuestionRandomizer.Infrastructure (External Services)
```
QuestionRandomizer.Infrastructure/
├── Firebase/
│   ├── FirebaseInitializer.cs       # Initialize Firebase Admin SDK
│   ├── FirebaseSettings.cs          # Firebase configuration options
│   └── FirestoreCollections.cs      # Collection name constants
│
├── Repositories/
│   ├── QuestionRepository.cs        # Implements IQuestionRepository
│   ├── CategoryRepository.cs        # Implements ICategoryRepository
│   ├── QualificationRepository.cs   # Implements IQualificationRepository
│   ├── ConversationRepository.cs    # Implements IConversationRepository
│   ├── MessageRepository.cs         # Implements IMessageRepository
│   └── BaseRepository.cs            # Shared Firestore operations
│
├── Services/
│   ├── AgentService.cs              # HTTP client to Agent Service
│   └── CurrentUserService.cs        # Extracts userId from HTTP context
│
├── Extensions/
│   ├── FirestoreExtensions.cs       # Helper methods for Firestore
│   └── DocumentSnapshotExtensions.cs
│
└── DependencyInjection.cs           # Register Infrastructure services
```

**Dependencies:** Domain, Application, Firebase Admin SDK, Google.Cloud.Firestore

#### 4. QuestionRandomizer.Api (HTTP Interface)
```
QuestionRandomizer.Api/
├── Controllers/
│   ├── QuestionsController.cs       # Question CRUD endpoints
│   ├── CategoriesController.cs      # Category CRUD endpoints
│   ├── QualificationsController.cs  # Qualification CRUD endpoints
│   ├── ConversationsController.cs   # Conversation & message endpoints
│   ├── AgentController.cs           # Agent task execution endpoints
│   └── AuthController.cs            # Token verification endpoint
│
├── Middleware/
│   ├── FirebaseAuthenticationMiddleware.cs  # Verify Firebase tokens
│   └── RequestLoggingMiddleware.cs          # Log all HTTP requests
│
├── Filters/
│   └── ValidateModelStateFilter.cs  # Automatic model validation
│
├── Extensions/
│   ├── ServiceCollectionExtensions.cs       # DI registration helpers
│   └── WebApplicationExtensions.cs          # Middleware configuration helpers
│
├── Program.cs                       # Application entry point
├── appsettings.json                 # Base configuration
├── appsettings.Development.json     # Development configuration
├── appsettings.Production.json      # Production configuration
└── Dockerfile                       # Docker container definition
```

**Dependencies:** All other projects, ASP.NET Core

#### 5. Test Projects
```
QuestionRandomizer.UnitTests/
├── Application/
│   ├── Commands/
│   │   └── CreateQuestionCommandHandlerTests.cs
│   ├── Queries/
│   │   └── GetQuestionsQueryHandlerTests.cs
│   └── Validators/
│       └── CreateQuestionCommandValidatorTests.cs
├── Domain/
│   └── Entities/
│       └── QuestionTests.cs
└── TestHelpers/
    └── FakeDataGenerator.cs         # Bogus fake data

QuestionRandomizer.IntegrationTests/
├── Controllers/
│   ├── QuestionsControllerTests.cs
│   └── CategoriesControllerTests.cs
├── Fixtures/
│   └── WebApplicationFactoryFixture.cs
└── TestHelpers/
    └── FirebaseEmulatorFixture.cs   # TestContainers setup

QuestionRandomizer.E2ETests/
├── Scenarios/
│   ├── QuestionManagementE2ETests.cs
│   └── AgentTaskE2ETests.cs
└── Fixtures/
    └── E2ETestFixture.cs            # Full system setup
```

---

## Implementation Phases

### Phase 1: Project Setup & Infrastructure (Day 1-2)
**Goal:** Create solution structure and configure foundational infrastructure

**Tasks:**
1. Create .NET solution and projects
2. Install all NuGet packages
3. Configure Firebase Admin SDK
4. Setup authentication middleware
5. Configure ProblemDetails error handling
6. Setup OpenTelemetry logging
7. Create basic Program.cs with DI

**Validation:** Project builds successfully, Firebase initializes without errors

---

### Phase 2: Domain Layer (Day 2)
**Goal:** Define core domain entities and repository interfaces

**Tasks:**
1. Create entity classes (Question, Category, etc.)
2. Define repository interfaces
3. Create custom domain exceptions
4. Add XML documentation to all public APIs

**Validation:** Domain project has zero external dependencies, builds successfully

---

### Phase 3: Application Layer - CQRS (Day 3-4)
**Goal:** Implement business logic using CQRS pattern

**Tasks:**
1. Create Commands with Handlers and Validators
2. Create Queries with Handlers
3. Define DTOs for all requests/responses
4. Implement MediatR pipeline behaviors
5. Configure MediatR in DI

**Validation:** Each command/query has handler + validator, MediatR pipeline configured

---

### Phase 4: Infrastructure Layer (Day 4-5)
**Goal:** Implement Firebase repositories and external services

**Tasks:**
1. Implement all repository classes
2. Create FirebaseInitializer with proper error handling
3. Implement AgentService HTTP client
4. Configure Polly retry policies
5. Register all services in DI

**Validation:** Can connect to Firestore, perform CRUD operations

---

### Phase 5: API Layer - Controllers (Day 5-6)
**Goal:** Create REST API endpoints

**Tasks:**
1. Create all Controllers
2. Configure authentication & authorization
3. Setup CORS policy
4. Configure Swagger/OpenAPI
5. Add rate limiting
6. Configure health checks

**Validation:** All endpoints documented in Swagger, authentication works

---

### Phase 6: Testing (Day 7-9)
**Goal:** Achieve comprehensive test coverage

**Tasks:**
1. Write unit tests for all handlers
2. Write unit tests for all validators
3. Setup TestContainers for integration tests
4. Write integration tests for all controllers
5. Create E2E test scenarios
6. Achieve >80% code coverage

**Validation:** All tests pass, coverage >80%

---

### Phase 7: Agent Service Integration (Day 10)
**Goal:** Integrate with TypeScript Agent Service

**Tasks:**
1. Implement AgentService HTTP client
2. Add streaming response handling
3. Configure timeout and cancellation
4. Add proper error handling
5. Create agent-specific DTOs

**Validation:** Can send tasks to agent service, handle responses

---

### Phase 8: Production Readiness (Day 11-12)
**Goal:** Prepare for deployment

**Tasks:**
1. Configure appsettings for all environments
2. Setup Azure Key Vault integration
3. Create Dockerfile with multi-stage build
4. Add health check endpoints
5. Configure logging levels
6. Performance testing
7. Security audit

**Validation:** Builds in Docker, health checks pass, ready for deployment

---

## Detailed Implementation Steps

### Step 1: Create Solution and Projects

```bash
# Navigate to project directory
cd C:\D\Repositories\question-randomizer-backend

# Create solution
dotnet new sln -n QuestionRandomizer

# Create Domain project (Class Library)
dotnet new classlib -n QuestionRandomizer.Domain -o src/QuestionRandomizer.Domain
dotnet sln add src/QuestionRandomizer.Domain

# Create Application project (Class Library)
dotnet new classlib -n QuestionRandomizer.Application -o src/QuestionRandomizer.Application
dotnet sln add src/QuestionRandomizer.Application

# Create Infrastructure project (Class Library)
dotnet new classlib -n QuestionRandomizer.Infrastructure -o src/QuestionRandomizer.Infrastructure
dotnet sln add src/QuestionRandomizer.Infrastructure

# Create API project (Web API)
dotnet new webapi -n QuestionRandomizer.Api -o src/QuestionRandomizer.Api
dotnet sln add src/QuestionRandomizer.Api

# Create Test projects
dotnet new xunit -n QuestionRandomizer.UnitTests -o tests/QuestionRandomizer.UnitTests
dotnet sln add tests/QuestionRandomizer.UnitTests

dotnet new xunit -n QuestionRandomizer.IntegrationTests -o tests/QuestionRandomizer.IntegrationTests
dotnet sln add tests/QuestionRandomizer.IntegrationTests

dotnet new xunit -n QuestionRandomizer.E2ETests -o tests/QuestionRandomizer.E2ETests
dotnet sln add tests/QuestionRandomizer.E2ETests

# Add project references
dotnet add src/QuestionRandomizer.Application reference src/QuestionRandomizer.Domain
dotnet add src/QuestionRandomizer.Infrastructure reference src/QuestionRandomizer.Domain
dotnet add src/QuestionRandomizer.Infrastructure reference src/QuestionRandomizer.Application
dotnet add src/QuestionRandomizer.Api reference src/QuestionRandomizer.Application
dotnet add src/QuestionRandomizer.Api reference src/QuestionRandomizer.Infrastructure

dotnet add tests/QuestionRandomizer.UnitTests reference src/QuestionRandomizer.Application
dotnet add tests/QuestionRandomizer.UnitTests reference src/QuestionRandomizer.Domain
dotnet add tests/QuestionRandomizer.IntegrationTests reference src/QuestionRandomizer.Api
dotnet add tests/QuestionRandomizer.E2ETests reference src/QuestionRandomizer.Api
```

### Step 2: Install NuGet Packages

**Domain Project** (no external dependencies - keep it pure!)
```bash
# Domain has NO external dependencies
```

**Application Project**
```bash
cd src/QuestionRandomizer.Application
dotnet add package MediatR --version 12.4.1
dotnet add package FluentValidation --version 11.10.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.10.0
```

**Infrastructure Project**
```bash
cd ../QuestionRandomizer.Infrastructure
dotnet add package FirebaseAdmin --version 3.0.1
dotnet add package Google.Cloud.Firestore --version 3.8.0
dotnet add package Microsoft.Extensions.Http.Polly --version 10.0.0
dotnet add package Polly --version 8.4.2
```

**API Project**
```bash
cd ../QuestionRandomizer.Api
dotnet add package Hellang.Middleware.ProblemDetails --version 6.5.1
dotnet add package Swashbuckle.AspNetCore --version 6.8.1
dotnet add package OpenTelemetry.Exporter.Console --version 1.9.0
dotnet add package OpenTelemetry.Extensions.Hosting --version 1.9.0
dotnet add package OpenTelemetry.Instrumentation.AspNetCore --version 1.9.0
dotnet add package AspNetCore.HealthChecks.Uris --version 10.0.0
```

**Unit Tests Project**
```bash
cd ../../tests/QuestionRandomizer.UnitTests
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Moq --version 4.20.72
dotnet add package Bogus --version 35.6.1
dotnet add package coverlet.collector --version 6.0.2
```

**Integration Tests Project**
```bash
cd ../QuestionRandomizer.IntegrationTests
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 10.0.0
dotnet add package Testcontainers --version 3.10.0
dotnet add package Bogus --version 35.6.1
```

**E2E Tests Project**
```bash
cd ../QuestionRandomizer.E2ETests
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 10.0.0
dotnet add package Testcontainers --version 3.10.0
```

### Step 3: Build Solution to Verify Setup

```bash
cd ../../..
dotnet build
```

Expected output: `Build succeeded. 0 Warning(s). 0 Error(s)`

---

## Code Templates and Patterns

### 1. Domain Entity Template

```csharp
// src/QuestionRandomizer.Domain/Entities/Question.cs
namespace QuestionRandomizer.Domain.Entities;

/// <summary>
/// Represents an interview question created by a user
/// </summary>
public class Question
{
    /// <summary>
    /// Unique identifier for the question (Firestore document ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The question text in English
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// The answer text in English
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// The answer text in Polish
    /// </summary>
    public string AnswerPl { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the category document ID (nullable)
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Denormalized category name for quick access
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Reference to the qualification document ID (nullable)
    /// </summary>
    public string? QualificationId { get; set; }

    /// <summary>
    /// Denormalized qualification name for quick access
    /// </summary>
    public string? QualificationName { get; set; }

    /// <summary>
    /// Whether the question is active (not deleted)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ID of the user who owns this question
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Optional tags for categorization
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// When the question was created (managed by Firestore)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the question was last updated (managed by Firestore)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
```

### 2. Repository Interface Template

```csharp
// src/QuestionRandomizer.Domain/Interfaces/IQuestionRepository.cs
namespace QuestionRandomizer.Domain.Interfaces;

using QuestionRandomizer.Domain.Entities;

/// <summary>
/// Repository contract for Question data access
/// </summary>
public interface IQuestionRepository
{
    /// <summary>
    /// Get all questions for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of questions</returns>
    Task<List<Question>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a question by its ID
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Question if found and authorized, null otherwise</returns>
    Task<Question?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new question
    /// </summary>
    /// <param name="question">Question to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created question with ID</returns>
    Task<Question> CreateAsync(Question question, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing question
    /// </summary>
    /// <param name="question">Question with updated data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateAsync(Question question, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a question (soft delete by setting IsActive = false)
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found or unauthorized</returns>
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get questions by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of questions in the category</returns>
    Task<List<Question>> GetByCategoryIdAsync(string categoryId, string userId, CancellationToken cancellationToken = default);
}
```

### 3. CQRS Command Template

```csharp
// src/QuestionRandomizer.Application/Commands/Questions/CreateQuestion/CreateQuestionCommand.cs
namespace QuestionRandomizer.Application.Commands.Questions.CreateQuestion;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Command to create a new question
/// </summary>
public record CreateQuestionCommand : IRequest<QuestionDto>
{
    public string QuestionText { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public string AnswerPl { get; init; } = string.Empty;
    public string? CategoryId { get; init; }
    public string? QualificationId { get; init; }
    public List<string>? Tags { get; init; }
}
```

### 4. Command Handler Template

```csharp
// src/QuestionRandomizer.Application/Commands/Questions/CreateQuestion/CreateQuestionCommandHandler.cs
namespace QuestionRandomizer.Application.Commands.Questions.CreateQuestion;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for CreateQuestionCommand
/// </summary>
public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, QuestionDto>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateQuestionCommandHandler(
        IQuestionRepository questionRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<QuestionDto> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Get category name if categoryId provided
        string? categoryName = null;
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, userId, cancellationToken);
            categoryName = category?.Name;
        }

        var question = new Question
        {
            QuestionText = request.QuestionText,
            Answer = request.Answer,
            AnswerPl = request.AnswerPl,
            CategoryId = request.CategoryId,
            CategoryName = categoryName,
            QualificationId = request.QualificationId,
            Tags = request.Tags,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdQuestion = await _questionRepository.CreateAsync(question, cancellationToken);

        return new QuestionDto
        {
            Id = createdQuestion.Id,
            QuestionText = createdQuestion.QuestionText,
            Answer = createdQuestion.Answer,
            AnswerPl = createdQuestion.AnswerPl,
            CategoryId = createdQuestion.CategoryId,
            CategoryName = createdQuestion.CategoryName,
            QualificationId = createdQuestion.QualificationId,
            QualificationName = createdQuestion.QualificationName,
            IsActive = createdQuestion.IsActive,
            Tags = createdQuestion.Tags
        };
    }
}
```

### 5. FluentValidation Validator Template

```csharp
// src/QuestionRandomizer.Application/Commands/Questions/CreateQuestion/CreateQuestionCommandValidator.cs
namespace QuestionRandomizer.Application.Commands.Questions.CreateQuestion;

using FluentValidation;

/// <summary>
/// Validator for CreateQuestionCommand
/// </summary>
public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required")
            .MaximumLength(1000).WithMessage("Question text must not exceed 1000 characters");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Answer is required")
            .MaximumLength(5000).WithMessage("Answer must not exceed 5000 characters");

        RuleFor(x => x.AnswerPl)
            .NotEmpty().WithMessage("Polish answer is required")
            .MaximumLength(5000).WithMessage("Polish answer must not exceed 5000 characters");

        RuleFor(x => x.CategoryId)
            .MaximumLength(100).WithMessage("Category ID must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CategoryId));

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 20)
            .WithMessage("Maximum 20 tags allowed")
            .When(x => x.Tags != null);
    }
}
```

### 6. CQRS Query Template

```csharp
// src/QuestionRandomizer.Application/Queries/Questions/GetQuestions/GetQuestionsQuery.cs
namespace QuestionRandomizer.Application.Queries.Questions.GetQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;

/// <summary>
/// Query to get all questions for the current user
/// </summary>
public record GetQuestionsQuery : IRequest<List<QuestionDto>>
{
    public string? CategoryId { get; init; }
    public bool? IsActive { get; init; }
}
```

### 7. Query Handler Template

```csharp
// src/QuestionRandomizer.Application/Queries/Questions/GetQuestions/GetQuestionsQueryHandler.cs
namespace QuestionRandomizer.Application.Queries.Questions.GetQuestions;

using MediatR;
using QuestionRandomizer.Application.DTOs;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Handler for GetQuestionsQuery
/// </summary>
public class GetQuestionsQueryHandler : IRequestHandler<GetQuestionsQuery, List<QuestionDto>>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetQuestionsQueryHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<QuestionDto>> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var questions = string.IsNullOrEmpty(request.CategoryId)
            ? await _questionRepository.GetByUserIdAsync(userId, cancellationToken)
            : await _questionRepository.GetByCategoryIdAsync(request.CategoryId, userId, cancellationToken);

        if (request.IsActive.HasValue)
        {
            questions = questions.Where(q => q.IsActive == request.IsActive.Value).ToList();
        }

        return questions.Select(q => new QuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            Answer = q.Answer,
            AnswerPl = q.AnswerPl,
            CategoryId = q.CategoryId,
            CategoryName = q.CategoryName,
            QualificationId = q.QualificationId,
            QualificationName = q.QualificationName,
            IsActive = q.IsActive,
            Tags = q.Tags
        }).ToList();
    }
}
```

### 8. Controller Template

```csharp
// src/QuestionRandomizer.Api/Controllers/QuestionsController.cs
namespace QuestionRandomizer.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Application.Commands.Questions.UpdateQuestion;
using QuestionRandomizer.Application.Commands.Questions.DeleteQuestion;
using QuestionRandomizer.Application.Queries.Questions.GetQuestions;
using QuestionRandomizer.Application.Queries.Questions.GetQuestionById;

/// <summary>
/// Manages interview questions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all questions for the authenticated user
    /// </summary>
    /// <param name="categoryId">Optional: filter by category</param>
    /// <param name="isActive">Optional: filter by active status</param>
    /// <returns>List of questions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionsQuery
        {
            CategoryId = categoryId,
            IsActive = isActive
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific question by ID
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <returns>Question details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionById(
        string id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new question
    /// </summary>
    /// <param name="command">Question details</param>
    /// <returns>Created question</returns>
    [HttpPost]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuestion(
        [FromBody] CreateQuestionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetQuestionById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing question
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <param name="command">Updated question details</param>
    /// <returns>Updated question</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(QuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuestion(
        string id,
        [FromBody] UpdateQuestionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in URL does not match ID in request body");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a question (soft delete)
    /// </summary>
    /// <param name="id">Question ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(
        string id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteQuestionCommand { Id = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
```

### 9. Repository Implementation Template

```csharp
// src/QuestionRandomizer.Infrastructure/Repositories/QuestionRepository.cs
namespace QuestionRandomizer.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;

/// <summary>
/// Firestore implementation of IQuestionRepository
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "questions";

    public QuestionRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<List<Question>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("userId", userId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc => doc.ConvertTo<Question>())
            .ToList();
    }

    public async Task<Question?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
        {
            return null;
        }

        var question = snapshot.ConvertTo<Question>();

        // Verify ownership
        if (question.UserId != userId)
        {
            return null;
        }

        return question;
    }

    public async Task<Question> CreateAsync(Question question, CancellationToken cancellationToken = default)
    {
        var docRef = await _firestoreDb.Collection(CollectionName).AddAsync(question, cancellationToken);
        question.Id = docRef.Id;
        return question;
    }

    public async Task<bool> UpdateAsync(Question question, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(question.Id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var existingQuestion = snapshot.ConvertTo<Question>();
        if (existingQuestion.UserId != question.UserId)
        {
            return false;
        }

        question.UpdatedAt = DateTime.UtcNow;
        await docRef.SetAsync(question, SetOptions.Overwrite, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var question = snapshot.ConvertTo<Question>();
        if (question.UserId != userId)
        {
            return false;
        }

        // Soft delete by setting IsActive = false
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "isActive", false },
            { "updatedAt", FieldValue.ServerTimestamp }
        }, cancellationToken: cancellationToken);

        return true;
    }

    public async Task<List<Question>> GetByCategoryIdAsync(string categoryId, string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("userId", userId)
            .WhereEqualTo("categoryId", categoryId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc => doc.ConvertTo<Question>())
            .ToList();
    }
}
```

### 10. Program.cs Template

```csharp
// src/QuestionRandomizer.Api/Program.cs
using QuestionRandomizer.Application;
using QuestionRandomizer.Infrastructure;
using Hellang.Middleware.ProblemDetails;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add ProblemDetails
builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();
});

// Add Application layer (MediatR, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure layer (Firebase, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("QuestionRandomizer.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

builder.Logging.AddOpenTelemetry(options =>
{
    options.AddConsoleExporter();
});

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseProblemDetails();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Make Program class accessible to tests
public partial class Program { }
```

### 11. Validation Pipeline Behavior

```csharp
// src/QuestionRandomizer.Application/Behaviors/ValidationBehavior.cs
namespace QuestionRandomizer.Application.Behaviors;

using FluentValidation;
using MediatR;

/// <summary>
/// MediatR pipeline behavior that validates commands/queries using FluentValidation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### 12. Unit Test Template

```csharp
// tests/QuestionRandomizer.UnitTests/Application/Commands/CreateQuestionCommandHandlerTests.cs
namespace QuestionRandomizer.UnitTests.Application.Commands;

using FluentAssertions;
using Moq;
using QuestionRandomizer.Application.Commands.Questions.CreateQuestion;
using QuestionRandomizer.Application.Interfaces;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;
using Xunit;

public class CreateQuestionCommandHandlerTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateQuestionCommandHandler _handler;

    public CreateQuestionCommandHandlerTests()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateQuestionCommandHandler(
            _questionRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesQuestion()
    {
        // Arrange
        const string userId = "user123";
        const string categoryId = "cat456";
        const string categoryName = "JavaScript";

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, Name = categoryName, UserId = userId });

        _questionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question q, CancellationToken ct) =>
            {
                q.Id = "q789";
                return q;
            });

        var command = new CreateQuestionCommand
        {
            QuestionText = "What is a closure?",
            Answer = "A closure is...",
            AnswerPl = "Domknięcie to...",
            CategoryId = categoryId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("q789");
        result.QuestionText.Should().Be(command.QuestionText);
        result.CategoryId.Should().Be(categoryId);
        result.CategoryName.Should().Be(categoryName);

        _questionRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<Question>(q =>
                q.QuestionText == command.QuestionText &&
                q.UserId == userId &&
                q.CategoryId == categoryId &&
                q.CategoryName == categoryName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoCategoryId_CreatesQuestionWithoutCategory()
    {
        // Arrange
        const string userId = "user123";

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _questionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question q, CancellationToken ct) =>
            {
                q.Id = "q789";
                return q;
            });

        var command = new CreateQuestionCommand
        {
            QuestionText = "What is a closure?",
            Answer = "A closure is...",
            AnswerPl = "Domknięcie to..."
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().BeNull();
        result.CategoryName.Should().BeNull();

        _categoryRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
```

---

## Configuration Guide

### appsettings.json (Base Configuration)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Firebase": {
    "ProjectId": "",
    "CredentialsPath": ""
  },
  "AgentService": {
    "BaseUrl": "http://localhost:3002",
    "TimeoutSeconds": 60
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:4200"
  }
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "Firebase": {
    "ProjectId": "your-dev-project-id",
    "CredentialsPath": "firebase-dev-credentials.json"
  },
  "AgentService": {
    "BaseUrl": "http://localhost:3002"
  }
}
```

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Firebase": {
    "ProjectId": "your-prod-project-id",
    "CredentialsPath": "/secrets/firebase-prod-credentials.json"
  },
  "AgentService": {
    "BaseUrl": "http://agent-service:3002"
  },
  "Cors": {
    "AllowedOrigins": "https://your-production-domain.com"
  }
}
```

### Environment Variables (for Production)

```bash
# Set in production environment (Azure App Service, Docker, etc.)
ASPNETCORE_ENVIRONMENT=Production
Firebase__ProjectId=your-prod-project-id
Firebase__CredentialsPath=/secrets/firebase-credentials.json
AgentService__BaseUrl=http://agent-service:3002
Cors__AllowedOrigins=https://your-production-domain.com
```

### Firebase Credentials Setup

1. **Get Service Account Key:**
   - Go to Firebase Console → Project Settings → Service Accounts
   - Click "Generate New Private Key"
   - Save as `firebase-dev-credentials.json` (for development)

2. **Store Credentials:**
   - Development: In project root (add to .gitignore!)
   - Production: Use Azure Key Vault or similar secret management

3. **Configure in Code:**
   ```csharp
   // Infrastructure/Firebase/FirebaseInitializer.cs
   var credentialsPath = configuration["Firebase:CredentialsPath"];
   Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
   FirebaseApp.Create(new AppOptions
   {
       ProjectId = configuration["Firebase:ProjectId"]
   });
   ```

---

## Testing Strategy

### Unit Tests (Fast, Isolated)

**What to Test:**
- Command/Query handlers
- Validators
- Domain logic
- Business rules

**Tools:**
- xUnit for test framework
- Moq for mocking dependencies
- FluentAssertions for readable assertions
- Bogus for fake data generation

**Example:**
```csharp
[Fact]
public async Task CreateQuestion_ValidInput_ReturnsQuestionDto()
{
    // Arrange - Setup mocks
    // Act - Call handler
    // Assert - Verify result
}
```

### Integration Tests (Medium Speed, Real Dependencies)

**What to Test:**
- API endpoints
- Firebase operations
- Authentication flow
- End-to-end request handling

**Tools:**
- WebApplicationFactory for TestServer
- TestContainers for Firebase Emulator
- Real HTTP requests

**Example:**
```csharp
public class QuestionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetQuestions_Authenticated_ReturnsOk()
    {
        // Arrange - Setup test server and auth
        // Act - Send HTTP request
        // Assert - Verify response
    }
}
```

### E2E Tests (Slow, Full System)

**What to Test:**
- Complete user flows
- Multi-step scenarios
- Agent service integration

**Example:**
```csharp
[Fact]
public async Task CompleteQuestionLifecycle_CreateUpdateDelete_Success()
{
    // 1. Authenticate
    // 2. Create question
    // 3. Update question
    // 4. Delete question
    // 5. Verify deletion
}
```

### Test Coverage Goals

- **Minimum:** 70% overall
- **Target:** 80% overall
- **Critical paths:** 95% (authentication, CRUD operations)

---

## API Endpoints Specification

### Authentication
```
POST /api/auth/verify
  - Verify Firebase ID token
  - Returns user info
```

### Questions
```
GET    /api/questions
  - Query params: ?categoryId=xxx&isActive=true
  - Returns: QuestionDto[]

GET    /api/questions/{id}
  - Returns: QuestionDto

POST   /api/questions
  - Body: CreateQuestionCommand
  - Returns: 201 Created with QuestionDto

PUT    /api/questions/{id}
  - Body: UpdateQuestionCommand
  - Returns: 200 OK with QuestionDto

DELETE /api/questions/{id}
  - Returns: 204 No Content
```

### Categories
```
GET    /api/categories
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}
```

### Qualifications
```
GET    /api/qualifications
POST   /api/qualifications
PUT    /api/qualifications/{id}
DELETE /api/qualifications/{id}
```

### Conversations
```
GET    /api/conversations
POST   /api/conversations
GET    /api/conversations/{id}/messages
DELETE /api/conversations/{id}
```

### Agent Tasks
```
POST   /api/agent/tasks
  - Body: { task: string }
  - Returns: Streaming JSON (agent messages)

GET    /api/agent/tasks/{id}
  - Returns: Task status and result
```

### Health
```
GET    /health
  - Returns: 200 OK if healthy
```

---

## Firebase Integration

### Firestore Collections

See ARCHITECTURE.md for complete schema. Key collections:

1. **users** - User profiles
2. **questions** - Interview questions
3. **categories** - Question categories
4. **qualifications** - Job qualifications
5. **conversations** - AI chat conversations
6. **messages** - Chat messages
7. **randomizations** - Randomization sessions
8. **selectedCategories** - Selected categories for session
9. **usedQuestions** - Questions shown in session
10. **postponedQuestions** - Questions postponed for later

### Firestore Indexes Required

```
questions:
  - userId (ascending)
  - userId (ascending), categoryId (ascending)
  - userId (ascending), isActive (ascending)

categories:
  - userId (ascending)

conversations:
  - userId (ascending), updatedAt (descending)

messages:
  - conversationId (ascending), timestamp (ascending)
```

Create indexes via Firebase Console or `firestore.indexes.json`

### Authentication Flow

1. User logs in via Firebase Auth (frontend)
2. Frontend receives Firebase ID token
3. Frontend sends token in Authorization header: `Bearer {token}`
4. Backend verifies token with Firebase Admin SDK
5. Backend extracts userId from verified token
6. Backend authorizes access to user's data

---

## Agent Service Integration

### Agent Service Overview

**Purpose:** Execute autonomous AI tasks using Claude Agent SDK

**Technology:** TypeScript, Node.js, Express.js

**Port:** 3002 (internal network)

### Integration Pattern

```csharp
// Infrastructure/Services/AgentService.cs
public interface IAgentService
{
    Task<AgentTaskResult> ExecuteTaskAsync(string task, string userId, CancellationToken cancellationToken);
    Task<AgentTaskStatus> GetTaskStatusAsync(string taskId, CancellationToken cancellationToken);
}

public class AgentService : IAgentService
{
    private readonly HttpClient _httpClient;

    public AgentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AgentTaskResult> ExecuteTaskAsync(string task, string userId, CancellationToken cancellationToken)
    {
        var request = new { task, userId };
        var response = await _httpClient.PostAsJsonAsync("/agent/task", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Handle streaming response
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        // Parse streaming JSON...

        return result;
    }
}
```

### Register in DI

```csharp
// Infrastructure/DependencyInjection.cs
services.AddHttpClient<IAgentService, AgentService>(client =>
{
    client.BaseAddress = new Uri(configuration["AgentService:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(
        int.Parse(configuration["AgentService:TimeoutSeconds"] ?? "60"));
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

### Error Handling

- **Timeout:** Return 408 Request Timeout to frontend
- **Agent Service Down:** Return 503 Service Unavailable
- **Task Failed:** Return 500 with error details from agent
- **Invalid Task:** Return 400 Bad Request

---

## Validation Checklist

### Phase 1: Project Setup ✓
- [ ] Solution created with 4 projects
- [ ] All NuGet packages installed
- [ ] Solution builds without errors
- [ ] Firebase credentials configured

### Phase 2: Domain Layer ✓
- [ ] All entity classes created
- [ ] All repository interfaces defined
- [ ] Domain exceptions created
- [ ] Domain project has zero external dependencies

### Phase 3: Application Layer ✓
- [ ] All commands created with handlers + validators
- [ ] All queries created with handlers
- [ ] All DTOs defined
- [ ] MediatR pipeline behaviors implemented
- [ ] Validation behavior works

### Phase 4: Infrastructure Layer ✓
- [ ] All repository implementations complete
- [ ] Firebase initialization works
- [ ] Agent service HTTP client configured
- [ ] Polly retry policies configured

### Phase 5: API Layer ✓
- [ ] All controllers created
- [ ] Authentication middleware works
- [ ] Swagger documentation generated
- [ ] CORS configured
- [ ] Health checks work

### Phase 6: Testing ✓
- [ ] Unit tests for all handlers (>80% coverage)
- [ ] Unit tests for all validators
- [ ] Integration tests for all controllers
- [ ] TestContainers Firebase Emulator works
- [ ] E2E tests for critical flows

### Phase 7: Agent Integration ✓
- [ ] Can send tasks to agent service
- [ ] Streaming responses handled
- [ ] Timeout mechanism works
- [ ] Error handling complete

### Phase 8: Production Ready ✓
- [ ] All environments configured
- [ ] Dockerfile builds successfully
- [ ] Health checks pass
- [ ] Logging configured
- [ ] Performance acceptable
- [ ] Security audit passed

---

## Commands Reference

### Build & Run
```bash
# Build solution
dotnet build

# Run API project
cd src/QuestionRandomizer.Api
dotnet run

# Run with hot reload
dotnet watch run

# Build for production
dotnet publish -c Release -o ./publish
```

### Testing
```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/QuestionRandomizer.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage
```

### Database
```bash
# Start Firebase Emulator (for local testing)
firebase emulators:start --only firestore

# Export Firestore data
gcloud firestore export gs://your-bucket/backup

# Import Firestore data
gcloud firestore import gs://your-bucket/backup
```

### Docker
```bash
# Build Docker image
docker build -t question-randomizer-api .

# Run container
docker run -p 3001:8080 -e ASPNETCORE_ENVIRONMENT=Production question-randomizer-api

# Docker Compose (with Firebase Emulator)
docker-compose up
```

---

## Next Steps After Context Clear

1. **Read ARCHITECTURE.md** - Understand system architecture
2. **Read this file (CLAUDE.md)** - Understand implementation plan
3. **Check current phase** - Look at file structure to see progress
4. **Resume from last completed phase** - Continue where we left off
5. **Run tests** - Verify everything still works
6. **Continue implementation** - Follow the detailed steps above

---

## Important Notes

### Security
- ⚠️ Never commit Firebase credentials to Git
- ⚠️ Always verify userId matches authenticated user
- ⚠️ Use HTTPS in production
- ⚠️ Implement rate limiting
- ⚠️ Validate all user input

### Performance
- ✅ Use async/await throughout
- ✅ Implement caching for rarely-changing data
- ✅ Use pagination for large result sets
- ✅ Monitor Firestore read/write costs

### Code Quality
- ✅ Write XML documentation for all public APIs
- ✅ Follow C# naming conventions
- ✅ Keep handlers thin (business logic in domain/services)
- ✅ One responsibility per class
- ✅ Prefer immutability (use records for DTOs)

---

**Last Updated:** 2025-11-22
**Status:** Ready for implementation - awaiting .NET 10 SDK installation
**Next Action:** Install .NET 10 SDK, then run Step 1 commands to create solution structure
