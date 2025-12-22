# Setup Guide - Question Randomizer Backend

Complete step-by-step guide to set up the Question Randomizer Backend project from scratch.

**Architecture:** Modular Monolith (migrated from Clean Architecture on 2025-12-22)
**Last Updated:** 2025-12-22

---

## Prerequisites

Before starting, ensure you have:

- ✅ **.NET 10 SDK** installed (`dotnet --version` → 10.x.x)
- ✅ **IDE:** Visual Studio 2025, JetBrains Rider, or VS Code with C# Dev Kit
- ✅ **Docker Desktop** (for integration tests with TestContainers)
- ✅ **Git** for version control

**NOTE:** This guide reflects the current Modular Monolith architecture. For the migration history from Clean Architecture, see [MIGRATION-SUMMARY.md](../MIGRATION-SUMMARY.md).

---

## Step 1: Create Solution and Projects

**Current Architecture:** Modular Monolith with vertical slices

The solution now consists of:
- **SharedKernel** - Cross-cutting concerns and domain events infrastructure
- **4 Business Modules** - Questions, Conversations, Randomization, Agent
- **2 API Projects** - Controllers and Minimal API
- **8 Test Projects** - Module tests + integration tests + E2E tests
- **3 Legacy Projects** - Domain, Application, Infrastructure (to be removed later)

```bash
# Navigate to project directory
cd C:\D\Repositories\question-randomizer-backend

# Create solution
dotnet new sln -n QuestionRandomizer

# ===== SHARED KERNEL =====
dotnet new classlib -n QuestionRandomizer.SharedKernel -o src/QuestionRandomizer.SharedKernel
dotnet sln add src/QuestionRandomizer.SharedKernel

# ===== BUSINESS MODULES =====
# Create Questions Module
dotnet new classlib -n QuestionRandomizer.Modules.Questions -o src/Modules/QuestionRandomizer.Modules.Questions
dotnet sln add src/Modules/QuestionRandomizer.Modules.Questions

# Create Conversations Module
dotnet new classlib -n QuestionRandomizer.Modules.Conversations -o src/Modules/QuestionRandomizer.Modules.Conversations
dotnet sln add src/Modules/QuestionRandomizer.Modules.Conversations

# Create Randomization Module
dotnet new classlib -n QuestionRandomizer.Modules.Randomization -o src/Modules/QuestionRandomizer.Modules.Randomization
dotnet sln add src/Modules/QuestionRandomizer.Modules.Randomization

# Create Agent Module
dotnet new classlib -n QuestionRandomizer.Modules.Agent -o src/Modules/QuestionRandomizer.Modules.Agent
dotnet sln add src/Modules/QuestionRandomizer.Modules.Agent

# ===== API PROJECTS =====
# Create Controllers API project (Web API)
dotnet new webapi -n QuestionRandomizer.Api.Controllers -o src/QuestionRandomizer.Api.Controllers
dotnet sln add src/QuestionRandomizer.Api.Controllers

# Create Minimal API project (Web API)
dotnet new webapi -n QuestionRandomizer.Api.MinimalApi -o src/QuestionRandomizer.Api.MinimalApi
dotnet sln add src/QuestionRandomizer.Api.MinimalApi

# ===== MODULE TEST PROJECTS =====
dotnet new xunit -n QuestionRandomizer.Modules.Questions.Tests -o tests/QuestionRandomizer.Modules.Questions.Tests
dotnet sln add tests/QuestionRandomizer.Modules.Questions.Tests

dotnet new xunit -n QuestionRandomizer.Modules.Conversations.Tests -o tests/QuestionRandomizer.Modules.Conversations.Tests
dotnet sln add tests/QuestionRandomizer.Modules.Conversations.Tests

dotnet new xunit -n QuestionRandomizer.Modules.Randomization.Tests -o tests/QuestionRandomizer.Modules.Randomization.Tests
dotnet sln add tests/QuestionRandomizer.Modules.Randomization.Tests

dotnet new xunit -n QuestionRandomizer.Modules.Agent.Tests -o tests/QuestionRandomizer.Modules.Agent.Tests
dotnet sln add tests/QuestionRandomizer.Modules.Agent.Tests

# ===== INTEGRATION & E2E TEST PROJECTS =====
dotnet new xunit -n QuestionRandomizer.IntegrationTests.Controllers -o tests/QuestionRandomizer.IntegrationTests.Controllers
dotnet sln add tests/QuestionRandomizer.IntegrationTests.Controllers

dotnet new xunit -n QuestionRandomizer.IntegrationTests.MinimalApi -o tests/QuestionRandomizer.IntegrationTests.MinimalApi
dotnet sln add tests/QuestionRandomizer.IntegrationTests.MinimalApi

dotnet new xunit -n QuestionRandomizer.E2ETests -o tests/QuestionRandomizer.E2ETests
dotnet sln add tests/QuestionRandomizer.E2ETests

dotnet new xunit -n QuestionRandomizer.UnitTests -o tests/QuestionRandomizer.UnitTests
dotnet sln add tests/QuestionRandomizer.UnitTests

# ===== LEGACY PROJECTS (Optional - for gradual migration) =====
# These can be skipped if starting fresh with modular monolith
# dotnet new classlib -n QuestionRandomizer.Domain -o src/QuestionRandomizer.Domain
# dotnet sln add src/QuestionRandomizer.Domain
# dotnet new classlib -n QuestionRandomizer.Application -o src/QuestionRandomizer.Application
# dotnet sln add src/QuestionRandomizer.Application
# dotnet new classlib -n QuestionRandomizer.Infrastructure -o src/QuestionRandomizer.Infrastructure
# dotnet sln add src/QuestionRandomizer.Infrastructure
```

**Total Projects:** 18 (5 modules + 2 APIs + 3 legacy + 8 tests)

---

## Step 2: Add Project References

**Modular Architecture Principle:** Modules only reference SharedKernel (never each other directly).

```bash
# ===== MODULES → SHAREDKERNEL =====
# Each module only references SharedKernel (no inter-module dependencies)
dotnet add src/Modules/QuestionRandomizer.Modules.Questions reference src/QuestionRandomizer.SharedKernel
dotnet add src/Modules/QuestionRandomizer.Modules.Conversations reference src/QuestionRandomizer.SharedKernel
dotnet add src/Modules/QuestionRandomizer.Modules.Randomization reference src/QuestionRandomizer.SharedKernel
dotnet add src/Modules/QuestionRandomizer.Modules.Agent reference src/QuestionRandomizer.SharedKernel

# Cross-module event reference (ONLY exception - for domain events)
# Randomization subscribes to CategoryDeletedEvent from Questions
dotnet add src/Modules/QuestionRandomizer.Modules.Randomization reference src/Modules/QuestionRandomizer.Modules.Questions

# ===== API PROJECTS → MODULES =====
# Controllers API references SharedKernel and all modules
dotnet add src/QuestionRandomizer.Api.Controllers reference src/QuestionRandomizer.SharedKernel
dotnet add src/QuestionRandomizer.Api.Controllers reference src/Modules/QuestionRandomizer.Modules.Questions
dotnet add src/QuestionRandomizer.Api.Controllers reference src/Modules/QuestionRandomizer.Modules.Conversations
dotnet add src/QuestionRandomizer.Api.Controllers reference src/Modules/QuestionRandomizer.Modules.Randomization
dotnet add src/QuestionRandomizer.Api.Controllers reference src/Modules/QuestionRandomizer.Modules.Agent

# Minimal API references SharedKernel and all modules
dotnet add src/QuestionRandomizer.Api.MinimalApi reference src/QuestionRandomizer.SharedKernel
dotnet add src/QuestionRandomizer.Api.MinimalApi reference src/Modules/QuestionRandomizer.Modules.Questions
dotnet add src/QuestionRandomizer.Api.MinimalApi reference src/Modules/QuestionRandomizer.Modules.Conversations
dotnet add src/QuestionRandomizer.Api.MinimalApi reference src/Modules/QuestionRandomizer.Modules.Randomization
dotnet add src/QuestionRandomizer.Api.MinimalApi reference src/Modules/QuestionRandomizer.Modules.Agent

# ===== MODULE TESTS → MODULES =====
# Each module test project references its module and SharedKernel
dotnet add tests/QuestionRandomizer.Modules.Questions.Tests reference src/QuestionRandomizer.SharedKernel
dotnet add tests/QuestionRandomizer.Modules.Questions.Tests reference src/Modules/QuestionRandomizer.Modules.Questions

dotnet add tests/QuestionRandomizer.Modules.Conversations.Tests reference src/QuestionRandomizer.SharedKernel
dotnet add tests/QuestionRandomizer.Modules.Conversations.Tests reference src/Modules/QuestionRandomizer.Modules.Conversations

dotnet add tests/QuestionRandomizer.Modules.Randomization.Tests reference src/QuestionRandomizer.SharedKernel
dotnet add tests/QuestionRandomizer.Modules.Randomization.Tests reference src/Modules/QuestionRandomizer.Modules.Randomization

dotnet add tests/QuestionRandomizer.Modules.Agent.Tests reference src/QuestionRandomizer.SharedKernel
dotnet add tests/QuestionRandomizer.Modules.Agent.Tests reference src/Modules/QuestionRandomizer.Modules.Agent

# ===== INTEGRATION TESTS → API PROJECTS =====
dotnet add tests/QuestionRandomizer.IntegrationTests.Controllers reference src/QuestionRandomizer.Api.Controllers
dotnet add tests/QuestionRandomizer.IntegrationTests.MinimalApi reference src/QuestionRandomizer.Api.MinimalApi

# ===== E2E TESTS → API PROJECT =====
dotnet add tests/QuestionRandomizer.E2ETests reference src/QuestionRandomizer.Api.Controllers

# ===== LEGACY UNIT TESTS (Optional) =====
# dotnet add tests/QuestionRandomizer.UnitTests reference src/QuestionRandomizer.Application
# dotnet add tests/QuestionRandomizer.UnitTests reference src/QuestionRandomizer.Domain
```

**Key Dependencies:**
- **Modules** → SharedKernel only (except Randomization → Questions for events)
- **APIs** → All Modules + SharedKernel
- **Module Tests** → Their Module + SharedKernel
- **Integration Tests** → API Projects

---

## Step 3: Install NuGet Packages

### SharedKernel Project
```bash
cd src/QuestionRandomizer.SharedKernel
dotnet add package MediatR --version 12.4.1
dotnet add package FirebaseAdmin --version 3.0.1
dotnet add package Google.Cloud.Firestore --version 3.8.0
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions
dotnet add package Microsoft.AspNetCore.Http
cd ../..
```

### Questions Module
```bash
cd src/Modules/QuestionRandomizer.Modules.Questions
dotnet add package MediatR --version 12.4.1
dotnet add package FluentValidation --version 11.10.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.10.0
dotnet add package Google.Cloud.Firestore --version 3.8.0
dotnet add package Microsoft.Extensions.Logging.Abstractions
cd ../../..
```

### Conversations Module
```bash
cd src/Modules/QuestionRandomizer.Modules.Conversations
dotnet add package MediatR --version 12.4.1
dotnet add package FluentValidation --version 11.10.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.10.0
dotnet add package Google.Cloud.Firestore --version 3.8.0
dotnet add package Microsoft.Extensions.Logging.Abstractions
cd ../../..
```

### Randomization Module
```bash
cd src/Modules/QuestionRandomizer.Modules.Randomization
dotnet add package MediatR --version 12.4.1
dotnet add package FluentValidation --version 11.10.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.10.0
dotnet add package Google.Cloud.Firestore --version 3.8.0
dotnet add package Microsoft.Extensions.Logging.Abstractions
cd ../../..
```

### Agent Module
```bash
cd src/Modules/QuestionRandomizer.Modules.Agent
dotnet add package MediatR --version 12.4.1
dotnet add package FluentValidation --version 11.10.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.10.0
dotnet add package Microsoft.Extensions.Http.Polly --version 10.0.0
dotnet add package Polly --version 8.4.2
dotnet add package Microsoft.Extensions.Logging.Abstractions
cd ../../..
```

### Controllers API Project
```bash
cd src/QuestionRandomizer.Api.Controllers
dotnet add package Hellang.Middleware.ProblemDetails --version 6.5.1
dotnet add package Swashbuckle.AspNetCore --version 6.8.1
dotnet add package OpenTelemetry.Exporter.Console --version 1.9.0
dotnet add package OpenTelemetry.Extensions.Hosting --version 1.9.0
dotnet add package OpenTelemetry.Instrumentation.AspNetCore --version 1.9.0
dotnet add package OpenTelemetry.Instrumentation.Http --version 1.14.0
dotnet add package AspNetCore.HealthChecks.Uris --version 9.0.0
dotnet add package Microsoft.OpenApi --version 3.0.1
cd ../..
```

### Minimal API Project
```bash
cd src/QuestionRandomizer.Api.MinimalApi
dotnet add package Hellang.Middleware.ProblemDetails --version 6.5.1
dotnet add package Swashbuckle.AspNetCore --version 6.8.1
dotnet add package OpenTelemetry.Exporter.Console --version 1.9.0
dotnet add package OpenTelemetry.Extensions.Hosting --version 1.9.0
dotnet add package OpenTelemetry.Instrumentation.AspNetCore --version 1.9.0
dotnet add package OpenTelemetry.Instrumentation.Http --version 1.14.0
dotnet add package AspNetCore.HealthChecks.Uris --version 9.0.0
dotnet add package Microsoft.OpenApi --version 3.0.1
cd ../..
```

### Module Test Projects

All module test projects use the same packages:

```bash
# Questions Module Tests
cd tests/QuestionRandomizer.Modules.Questions.Tests
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Moq --version 4.20.72
dotnet add package Bogus --version 35.6.5
dotnet add package coverlet.collector --version 6.0.2
cd ../..

# Conversations Module Tests
cd tests/QuestionRandomizer.Modules.Conversations.Tests
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Moq --version 4.20.72
dotnet add package Bogus --version 35.6.5
dotnet add package coverlet.collector --version 6.0.2
cd ../..

# Randomization Module Tests
cd tests/QuestionRandomizer.Modules.Randomization.Tests
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Moq --version 4.20.72
dotnet add package Bogus --version 35.6.5
dotnet add package coverlet.collector --version 6.0.2
cd ../..

# Agent Module Tests
cd tests/QuestionRandomizer.Modules.Agent.Tests
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Moq --version 4.20.72
dotnet add package Bogus --version 35.6.5
dotnet add package coverlet.collector --version 6.0.2
cd ../..
```

### Legacy Unit Tests Project (Optional)
```bash
cd tests/QuestionRandomizer.UnitTests
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Moq --version 4.20.72
dotnet add package Bogus --version 35.6.1
dotnet add package coverlet.collector --version 6.0.2
cd ../..
```

### Integration Tests Project
```bash
cd tests/QuestionRandomizer.IntegrationTests.Controllers
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 10.0.0
dotnet add package Testcontainers --version 3.10.0
dotnet add package Bogus --version 35.6.1
cd ../..
```

### E2E Tests Project
```bash
cd tests/QuestionRandomizer.E2ETests
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package FluentAssertions --version 7.0.0
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 10.0.0
dotnet add package Testcontainers --version 3.10.0
cd ../..
```

---

## Step 4: Verify Setup

```bash
# Build entire solution
dotnet build
```

**Expected output:** `Build succeeded. 0 Warning(s). 0 Error(s)`

If build fails, check:
- All project references are correct
- NuGet packages installed successfully
- .NET 10 SDK is installed

---

## Step 5: Configure Firebase

### Get Service Account Key

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Select your project
3. Go to **Project Settings** → **Service Accounts**
4. Click **"Generate New Private Key"**
5. Save as `firebase-dev-credentials.json` in project root
6. **IMPORTANT:** Add to `.gitignore` (should already be there)

### Update appsettings.Development.json

**Controllers API:**
```json
// src/QuestionRandomizer.Api.Controllers/appsettings.Development.json
{
  "Firebase": {
    "ProjectId": "your-firebase-project-id",
    "CredentialsPath": "firebase-dev-credentials.json"
  }
}
```

**Minimal API:**
```json
// src/QuestionRandomizer.Api.MinimalApi/appsettings.Development.json
{
  "Firebase": {
    "ProjectId": "your-firebase-project-id",
    "CredentialsPath": "firebase-dev-credentials.json"
  }
}
```

---

## Step 6: Run the APIs

### Run Controllers API (Port 5000)

```bash
cd src/QuestionRandomizer.Api.Controllers
dotnet run
```

- Swagger UI: http://localhost:5000/swagger
- Health Check: http://localhost:5000/health

### Run Minimal API (Port 5001)

**In a separate terminal:**

```bash
cd src/QuestionRandomizer.Api.MinimalApi
dotnet run
```

- Swagger UI: http://localhost:5001/swagger
- Health Check: http://localhost:5001/health

### Run with Hot Reload

```bash
# Controllers API
cd src/QuestionRandomizer.Api.Controllers
dotnet watch run

# Minimal API (separate terminal)
cd src/QuestionRandomizer.Api.MinimalApi
dotnet watch run
```

---

## Step 7: Run Tests

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test tests/QuestionRandomizer.UnitTests

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage
```

---

## Common Issues

### Issue: "Firebase credentials not found"
**Solution:** Ensure `firebase-dev-credentials.json` exists and path in `appsettings.Development.json` is correct.

### Issue: "Port 5000 already in use"
**Solution:** Change port in `appsettings.Development.json` or stop the process using port 5000.

### Issue: "FirebaseAdmin package not found"
**Solution:** Restore NuGet packages:
```bash
dotnet restore
```

### Issue: "Build failed with SDK errors"
**Solution:** Verify .NET 10 SDK is installed:
```bash
dotnet --version  # Should show 10.x.x
```

---

## Next Steps

1. **Read [CLAUDE.md](../CLAUDE.md)** - Developer guide with architecture overview
2. **Read [CODE-TEMPLATES.md](./CODE-TEMPLATES.md)** - Code patterns and examples
3. **Read [CONFIGURATION.md](./CONFIGURATION.md)** - Configuration details
4. **Read [DUAL-API-GUIDE.md](./DUAL-API-GUIDE.md)** - Controllers vs Minimal API
5. **Read [TESTING.md](./TESTING.md)** - Testing strategy and examples
6. **Start developing** - Follow the Getting Started guide in CLAUDE.md

---

## Quick Reference

```bash
# Build solution
dotnet build

# Run Controllers API
cd src/QuestionRandomizer.Api.Controllers && dotnet run

# Run Minimal API
cd src/QuestionRandomizer.Api.MinimalApi && dotnet run

# Run all tests
dotnet test

# Add new package
dotnet add package <PackageName>

# Add project reference
dotnet add reference <ProjectPath>
```
