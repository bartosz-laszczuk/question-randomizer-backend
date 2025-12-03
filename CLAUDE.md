# Question Randomizer Backend - Developer Guide

**Project:** Question Randomizer Backend API (Dual Implementation)
**Technology:** .NET 10 (C#)
**Architecture:** Clean Architecture + CQRS + MediatR
**Database:** Firebase Firestore
**Authentication:** Firebase Authentication
**Last Updated:** 2025-12-03

---

## 📚 Documentation Index

- **[CODE-TEMPLATES.md](./docs/CODE-TEMPLATES.md)** - All code templates and patterns
- **[SETUP-GUIDE.md](./docs/SETUP-GUIDE.md)** - Complete step-by-step setup instructions
- **[CONFIGURATION.md](./docs/CONFIGURATION.md)** - Configuration details and examples
- **[DUAL-API-GUIDE.md](./docs/DUAL-API-GUIDE.md)** - Controllers vs Minimal API comparison
- **[TESTING.md](./docs/TESTING.md)** - Testing strategy and examples
- **[DEPLOYMENT.md](./docs/DEPLOYMENT.md)** - Deployment guide for Docker, Azure, AWS, Kubernetes

---

## Project Overview

### Purpose
Build a .NET 10 backend API that serves as the orchestration layer for the Question Randomizer application. This API handles:
- User authentication and authorization via Firebase
- CRUD operations for questions, categories, qualifications
- Orchestration of AI agent tasks
- Conversation and message management

### Key Features
- Modern .NET 10 best practices
- Clean, maintainable architecture with CQRS pattern
- Comprehensive test coverage (Unit, Integration, E2E)
- Production-ready with proper error handling
- High performance and scalability
- Type-safe throughout with strong validation

### System Context
This backend is part of a 3-service architecture:
1. **Angular Frontend** (existing) - User interface
2. **C# Backend API** (this project) - Main API and orchestration
3. **TypeScript Agent Service** - AI-powered autonomous tasks

**Frontend → Backend API → [Firestore, Agent Service]**

**Agent Integration:** The C# Backend now communicates with the TypeScript AI Agent Service via:
- Synchronous execution (POST /agent/task)
- Streaming execution with SSE (POST /agent/task/stream)
- Queue-based async execution (POST /agent/task/queue)

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
├── CLAUDE.md                                           # This file - developer guide
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
- **Two complete API implementations** running side-by-side
- **Same business logic** - Domain, Application, Infrastructure shared
- **Different ports** - 5000 (Controllers), 5001 (Minimal API)
- **Perfect learning tool** - Compare approaches with identical functionality

---

## Quick Start

### Prerequisites
- .NET 10 SDK (`dotnet --version` → 10.x.x)
- IDE: Visual Studio 2025, JetBrains Rider, or VS Code with C# Dev Kit
- Docker Desktop (for integration tests)
- Firebase project with credentials

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
GET    /api/categories/{id}       # Get category by ID
POST   /api/categories            # Create category
POST   /api/categories/batch      # Create multiple categories
PUT    /api/categories/{id}       # Update category
DELETE /api/categories/{id}       # Delete category
```

### Qualifications
```
GET    /api/qualifications        # List all qualifications
GET    /api/qualifications/{id}   # Get qualification by ID
POST   /api/qualifications        # Create qualification
POST   /api/qualifications/batch  # Create multiple qualifications
PUT    /api/qualifications/{id}   # Update qualification
DELETE /api/qualifications/{id}   # Delete qualification
```

### Conversations
```
GET    /api/conversations         # List all conversations
GET    /api/conversations/{id}    # Get conversation by ID
POST   /api/conversations         # Create conversation
POST   /api/conversations/{id}/messages  # Add message to conversation
PUT    /api/conversations/{id}    # Update conversation
DELETE /api/conversations/{id}    # Delete conversation
```

### Randomization
```
POST   /api/randomization/randomize        # Get random questions
POST   /api/randomization/randomize/batch  # Get multiple sets of questions
GET    /api/randomization/history          # Get randomization history
GET    /api/randomization/history/{id}     # Get specific randomization
```

### Agent Tasks
```
POST   /api/agent/execute         # Execute AI agent task (synchronous)
POST   /api/agent/execute/stream  # Execute with streaming progress (SSE)
POST   /api/agent/queue           # Queue task for background processing
GET    /api/agent/tasks/{id}      # Get task status/result
```

### Health
```
GET    /health                    # Health check endpoint
```

**All endpoints are available on both APIs (Controllers: Port 5000, Minimal API: Port 5001) with identical functionality.**

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

## Getting Started

### For New Developers

1. **Understand the Architecture**
   - Review the "Architecture Decisions" section above (Clean Architecture, CQRS, Dual API)
   - Examine the "Project Structure" to understand the solution layout
   - Read [DUAL-API-GUIDE.md](./docs/DUAL-API-GUIDE.md) for Controllers vs Minimal API comparison

2. **Set Up Your Environment**
   - Follow [SETUP-GUIDE.md](./docs/SETUP-GUIDE.md) for complete setup
   - Configure Firebase credentials (see Configuration section)
   - Run `dotnet build` to verify everything works

3. **Explore the Codebase**
   - Review [CODE-TEMPLATES.md](./docs/CODE-TEMPLATES.md) to understand code patterns
   - Start with Domain layer → Application layer → Infrastructure layer
   - Examine both API implementations side-by-side

4. **Run and Test**
   - Start Controllers API: `cd src/QuestionRandomizer.Api.Controllers && dotnet run`
   - Start Minimal API: `cd src/QuestionRandomizer.Api.MinimalApi && dotnet run`
   - Run tests: `dotnet test`
   - Explore Swagger UI: http://localhost:5000 (Controllers), http://localhost:5001 (Minimal API)

5. **Making Changes**
   - Follow established code patterns (see CODE-TEMPLATES.md)
   - Write tests for new features
   - Update both API implementations if adding new endpoints
   - Review [TESTING.md](./docs/TESTING.md) for testing guidelines

---

## Additional Resources

### Documentation
- **[DEPLOYMENT.md](./docs/DEPLOYMENT.md)** - Deployment guide for Docker, Azure, AWS, Kubernetes
- **[SECURITY-AUDIT.md](./docs/SECURITY-AUDIT.md)** - Security checklist and best practices
- **[INTEGRATION-TEST-SUMMARY.md](./INTEGRATION-TEST-SUMMARY.md)** - Detailed test results and coverage

### CI/CD & DevOps
- GitHub Actions workflow configured for automated builds, tests, and deployments
- Docker support with multi-stage builds for both APIs
- Health checks, logging, and monitoring configured
- Environment-specific configuration (Development, Staging, Production)

### Support & Contributing
- Report issues or request features via the project's issue tracker
- Follow the established code patterns when contributing
- Ensure all tests pass before submitting changes
- Update both API implementations for consistency
