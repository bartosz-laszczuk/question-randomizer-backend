# Question Randomizer Backend - Implementation Guide

**Project:** Question Randomizer Backend API (Dual Implementation)
**Technology:** .NET 10 (C#)
**Architecture:** Clean Architecture + CQRS + MediatR
**Database:** Firebase Firestore
**Authentication:** Firebase Authentication
**Last Updated:** 2025-11-24
**Status:** Phases 1-5 Complete - **DUAL API IMPLEMENTATION** (Controllers + Minimal API)

---

## 📚 Documentation Index

- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - System architecture, database schema, data flow
- **[CODE-TEMPLATES.md](./docs/CODE-TEMPLATES.md)** - All code templates and patterns
- **[SETUP-GUIDE.md](./docs/SETUP-GUIDE.md)** - Complete step-by-step setup instructions
- **[CONFIGURATION.md](./docs/CONFIGURATION.md)** - Configuration details and examples
- **[DUAL-API-GUIDE.md](./docs/DUAL-API-GUIDE.md)** - Controllers vs Minimal API comparison
- **[TESTING.md](./docs/TESTING.md)** - Testing strategy and examples

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

## Architecture Decisions

### Dual API Implementation 🎓
**Decision:** Implement BOTH Controllers and Minimal APIs side-by-side

**Implementation:**
- `QuestionRandomizer.Api.Controllers` (Port 5000) - Traditional Controllers approach
- `QuestionRandomizer.Api.MinimalApi` (Port 5001) - Modern Minimal API approach
- **Same Domain, Application, and Infrastructure layers** - Perfect demonstration of Clean Architecture

**Rationale:**
- Learning Purpose: Compare both approaches in production-quality code
- Clean Architecture Demonstration: Show how presentation layer can be swapped
- Performance Comparison: Benchmark real-world performance differences
- Team Training: Developers can learn both patterns with identical functionality

**📖 See [DUAL-API-GUIDE.md](./docs/DUAL-API-GUIDE.md) for detailed comparison and when to use each approach.**

### Why Clean Architecture?
**Structure:**
```
Domain ← Application ← Infrastructure
                ↑
               API
```

**Benefits:**
- Clear separation of concerns
- Dependency inversion (dependencies point inward)
- Highly testable (business logic isolated from infrastructure)
- Technology-agnostic domain layer

### Why CQRS with MediatR?
**Pattern:**
```
Controller → MediatR → Command/Query Handler → Repository → Firestore
```

**Benefits:**
- Separates read and write operations
- Easy to test handlers in isolation
- Decouples controllers from business logic
- Supports cross-cutting concerns (logging, validation)

---

## Technology Stack

### Core Framework
- **.NET 10 LTS** - Latest long-term support (3 years until Nov 2028)
- **ASP.NET Core 10** - Web framework
- **C# 14** - Latest language features

### Key Libraries
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Input validation
- **FirebaseAdmin** - Firebase integration
- **Google.Cloud.Firestore** - Firestore client
- **Polly** - Resilience and retry policies
- **OpenTelemetry** - Observability
- **Swashbuckle** - Swagger/OpenAPI documentation

**📖 See [SETUP-GUIDE.md](./docs/SETUP-GUIDE.md) for complete package list and installation commands.**

---

## Project Structure

### Solution Structure (Dual API Implementation)
```
question-randomizer-backend/
├── QuestionRandomizer.sln                              # Solution file (8 projects)
├── ARCHITECTURE.md                                     # System architecture
├── CLAUDE.md                                           # This file - quick reference
├── docs/                                               # Detailed documentation
│   ├── CODE-TEMPLATES.md
│   ├── SETUP-GUIDE.md
│   ├── CONFIGURATION.md
│   ├── DUAL-API-GUIDE.md
│   └── TESTING.md
│
├── src/
│   ├── QuestionRandomizer.Domain/                     # 🏛️ Domain Layer (Entities, Interfaces)
│   ├── QuestionRandomizer.Application/                # 💼 Application Layer (CQRS, Business Logic)
│   ├── QuestionRandomizer.Infrastructure/             # 🔧 Infrastructure Layer (Firebase, External Services)
│   ├── QuestionRandomizer.Api.Controllers/            # 🎯 Controllers API (Port 5000)
│   └── QuestionRandomizer.Api.MinimalApi/             # 🚀 Minimal API (Port 5001)
│
└── tests/
    ├── QuestionRandomizer.UnitTests/                  # 🧪 Unit Tests
    ├── QuestionRandomizer.IntegrationTests.Controllers/ # 🔗 Integration Tests
    └── QuestionRandomizer.E2ETests/                   # 🎭 End-to-End Tests
```

**Key Points:**
- ✅ **Two complete API implementations** running side-by-side
- ✅ **Same business logic** - Domain, Application, Infrastructure shared
- ✅ **Different ports** - 5000 (Controllers), 5001 (Minimal API)
- ✅ **Perfect learning tool** - Compare approaches with identical functionality

**📖 See [ARCHITECTURE.md](./ARCHITECTURE.md) for detailed project breakdown.**

---

## Quick Start

### Prerequisites
- ✅ .NET 10 SDK (`dotnet --version` → 10.x.x)
- ✅ IDE: Visual Studio 2025, JetBrains Rider, or VS Code with C# Dev Kit
- ✅ Docker Desktop (for integration tests)
- ✅ Firebase project with credentials

### Build & Run

```bash
# Build entire solution
dotnet build

# Run Controllers API (Port 5000)
cd src/QuestionRandomizer.Api.Controllers
dotnet run
# Swagger UI: http://localhost:5000

# Run Minimal API (Port 5001) - in separate terminal
cd src/QuestionRandomizer.Api.MinimalApi
dotnet run
# Swagger UI: http://localhost:5001
```

**📖 See [SETUP-GUIDE.md](./docs/SETUP-GUIDE.md) for complete setup instructions.**

---

## Implementation Phases

### Phase 1: Project Setup & Infrastructure ✅
- [x] Solution created with 8 projects (5 src + 3 test projects)
- [x] All NuGet packages installed (.NET 10 SDK version 10.0.100)
- [x] Solution builds without errors
- [ ] Firebase credentials configured (placeholder values in appsettings)

### Phase 2: Domain Layer ✅
- [x] All entity classes created (7 entities)
- [x] All repository interfaces defined (6 interfaces)
- [x] Domain exceptions created (4 exception types)
- [x] Domain project has zero external dependencies

### Phase 3: Application Layer ✅
- [x] Commands created with handlers + validators (3 commands)
- [x] Queries created with handlers (2 queries)
- [x] All DTOs defined (5 DTOs)
- [x] MediatR pipeline behaviors implemented (ValidationBehavior, LoggingBehavior)

### Phase 4: Infrastructure Layer ✅
- [x] All repository implementations complete (6 repositories)
- [x] Firebase initialization works (FirebaseSettings, FirestoreCollections, DependencyInjection)
- [x] Agent service HTTP client configured (AgentService with HttpClient)
- [x] Polly retry policies configured (3 retries with exponential backoff)

### Phase 5A: API Layer - Controllers (Port 5000) ✅
- [x] QuestionsController created (complete CRUD operations - 5 endpoints)
- [x] Authentication middleware configured
- [x] Swagger documentation generated
- [x] CORS configured
- [x] Health checks configured
- [x] Port configured to 5000 in Development

### Phase 5B: API Layer - Minimal API (Port 5001) ✅
- [x] QuestionEndpoints created (5 endpoints using MapGroup() and TypedResults)
- [x] Authentication configured with RequireAuthorization()
- [x] Swagger documentation generated
- [x] Port configured to 5001 in Development

### Phase 6: Testing 🔲
- [ ] Unit tests for all handlers (>80% coverage)
- [ ] Unit tests for all validators
- [ ] Integration tests for all controllers
- [ ] TestContainers Firebase Emulator works
- [ ] E2E tests for critical flows

**📖 See [TESTING.md](./docs/TESTING.md) for testing strategy and examples.**

### Phase 7: Agent Integration 🔲
- [ ] Can send tasks to agent service
- [ ] Streaming responses handled
- [ ] Timeout mechanism works
- [ ] Error handling complete

### Phase 8: Production Ready 🔲
- [ ] All environments configured
- [ ] Dockerfile builds successfully
- [ ] Health checks pass
- [ ] Logging configured
- [ ] Performance acceptable
- [ ] Security audit passed

---

## Code Patterns

All code follows established templates. See **[CODE-TEMPLATES.md](./docs/CODE-TEMPLATES.md)** for complete examples.

**Quick Reference:**
- **Domain Entities:** POCOs with XML docs, zero dependencies
- **Repository Interfaces:** Task-based async methods
- **CQRS Commands:** `IRequest<TResponse>` records
- **Command Handlers:** `IRequestHandler<TRequest, TResponse>`
- **FluentValidation:** `AbstractValidator<T>` with rule chains
- **Controllers:** `[ApiController]` with MediatR injection
- **Minimal API:** Static extension methods with MapGroup()

**Example Command Handler:**
```csharp
public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, QuestionDto>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public async Task<QuestionDto> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var question = new Question { /* map from request */ };
        var created = await _questionRepository.CreateAsync(question, cancellationToken);
        return /* map to DTO */;
    }
}
```

---

## Configuration

**Key Settings:**
- **Firebase:** ProjectId, CredentialsPath
- **AgentService:** BaseUrl (http://localhost:3002), TimeoutSeconds (60)
- **CORS:** AllowedOrigins (http://localhost:4200)
- **Ports:** 5000 (Controllers), 5001 (Minimal API)

**Environment-specific files:**
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Dev overrides (includes port configuration)
- `appsettings.Production.json` - Prod configuration

**Firebase Credentials:**
1. Get service account key from Firebase Console
2. Save as `firebase-dev-credentials.json` in project root
3. **IMPORTANT:** Add to `.gitignore` (never commit!)
4. Update `appsettings.Development.json` with path

**📖 See [CONFIGURATION.md](./docs/CONFIGURATION.md) for detailed configuration examples.**

---

## API Endpoints

### Questions
```
GET    /api/questions             # List all questions
GET    /api/questions/{id}        # Get question by ID
POST   /api/questions             # Create question
PUT    /api/questions/{id}        # Update question
DELETE /api/questions/{id}        # Delete question (soft delete)
```

### Categories
```
GET    /api/categories            # List all categories
POST   /api/categories            # Create category
PUT    /api/categories/{id}       # Update category
DELETE /api/categories/{id}       # Delete category
```

### Qualifications
```
GET    /api/qualifications        # List all qualifications
POST   /api/qualifications        # Create qualification
PUT    /api/qualifications/{id}   # Update qualification
DELETE /api/qualifications/{id}   # Delete qualification
```

### Agent Tasks
```
POST   /api/agent/tasks           # Execute AI agent task
GET    /api/agent/tasks/{id}      # Get task status/result
```

### Health
```
GET    /health                    # Health check endpoint
```

**📖 See [ARCHITECTURE.md](./ARCHITECTURE.md) for complete API specification.**

---

## Testing Strategy

### Test Layers
1. **Unit Tests** - Fast, isolated tests for handlers and validators
2. **Integration Tests** - Test API endpoints with real dependencies
3. **E2E Tests** - Test complete user workflows

**Coverage Goals:**
- Minimum: 70% overall
- Target: 80% overall
- Critical paths: 95% (authentication, CRUD operations)

**Tools:**
- xUnit - Test framework
- Moq - Mocking dependencies
- FluentAssertions - Readable assertions
- Bogus - Fake data generation
- TestContainers - Docker containers for dependencies

**Run Tests:**
```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/QuestionRandomizer.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**📖 See [TESTING.md](./docs/TESTING.md) for complete testing guide.**

---

## Commands Reference

### Build & Run
```bash
# Build entire solution
dotnet build

# Run Controllers API (Port 5000)
cd src/QuestionRandomizer.Api.Controllers && dotnet run

# Run Minimal API (Port 5001)
cd src/QuestionRandomizer.Api.MinimalApi && dotnet run

# Run with hot reload
dotnet watch run

# Build for production
dotnet publish -c Release
```

### Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Generate coverage report
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage
```

### Database
```bash
# Start Firebase Emulator (for local testing)
firebase emulators:start --only firestore
```

**📖 See [SETUP-GUIDE.md](./docs/SETUP-GUIDE.md) for detailed commands.**

---

## Dual API Implementation 🎓

This project implements **both Controllers and Minimal APIs** for comparison.

**Quick Comparison:**

| Aspect | Controllers (5000) | Minimal API (5001) |
|--------|-------------------|-------------------|
| Style | OOP, class-based | Functional |
| Boilerplate | More | Less |
| Performance | Baseline | ~5-10% faster |
| Best For | Large apps, complex filters | Microservices, new projects |

**Testing:**
```bash
# Test identical functionality on both APIs
curl http://localhost:5000/api/questions  # Controllers
curl http://localhost:5001/api/questions  # Minimal API
# Result: Identical JSON responses!
```

**📖 See [DUAL-API-GUIDE.md](./docs/DUAL-API-GUIDE.md) for detailed comparison and side-by-side code examples.**

---

## Security Notes

### ⚠️ NEVER:
- Commit Firebase credentials to Git
- Use production credentials in development
- Hardcode secrets in code
- Share credentials in plain text

### ✅ ALWAYS:
- Verify userId matches authenticated user
- Use HTTPS in production
- Implement rate limiting
- Validate all user input
- Store secrets in secure vaults (Azure Key Vault, etc.)

---

## Troubleshooting

### Issue: "Firebase credentials not found"
**Solution:** Ensure `firebase-dev-credentials.json` exists and path in `appsettings.Development.json` is correct.

### Issue: "Port 5000 already in use"
**Solution:** Change port in `appsettings.Development.json` or stop the process using port 5000.

### Issue: "Build failed with SDK errors"
**Solution:** Verify .NET 10 SDK is installed: `dotnet --version` → should show 10.x.x

---

## Next Steps

1. ✅ **Read [ARCHITECTURE.md](./ARCHITECTURE.md)** - Understand system architecture
2. ✅ **Read [SETUP-GUIDE.md](./docs/SETUP-GUIDE.md)** - Complete setup
3. ✅ **Read [CODE-TEMPLATES.md](./docs/CODE-TEMPLATES.md)** - Code patterns
4. ✅ **Configure Firebase** - Add credentials to appsettings
5. ✅ **Run both APIs** - Test Controllers (5000) and Minimal API (5001)
6. ✅ **Start Phase 6** - Implement unit, integration, and E2E tests

---

## Implementation Notes

### What Was Built (Phases 1-5 Complete)
1. **Domain Layer**: 7 entities, 6 repository interfaces, 4 custom exceptions (zero external dependencies) ✅
2. **Application Layer**: CQRS with MediatR (3 commands, 2 queries), FluentValidation, 2 pipeline behaviors ✅
3. **Infrastructure Layer**: 6 Firestore repositories, Firebase Admin SDK integration, AgentService with Polly retry policies ✅
4. **Controllers API (Port 5000)**: QuestionsController with 5 CRUD endpoints, full middleware pipeline, Swagger documentation ✅
5. **Minimal API (Port 5001)**: QuestionEndpoints with 5 endpoints using MapGroup() and TypedResults, same middleware pipeline ✅

### Remaining Work
- **Phase 6**: Unit, Integration, and E2E tests for both APIs
- **Phase 7**: Additional endpoints (Categories, Qualifications, Conversations, Agent) in both APIs
- **Phase 8**: Production readiness (Docker, environment configuration, security audit)

---

**Last Updated:** 2025-11-24
**Status:** Phases 1-5 Complete - **DUAL API IMPLEMENTATION FINISHED** ✨
**Next Action:** Configure Firebase credentials in appsettings, then proceed with Phase 6 (Testing)
