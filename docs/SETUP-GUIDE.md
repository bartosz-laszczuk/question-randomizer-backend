# Setup Guide - Question Randomizer Backend

Complete step-by-step guide to set up the Question Randomizer Backend project from scratch.

---

## Prerequisites

Before starting, ensure you have:

- ✅ **.NET 10 SDK** installed (`dotnet --version` → 10.x.x)
- ✅ **IDE:** Visual Studio 2025, JetBrains Rider, or VS Code with C# Dev Kit
- ✅ **Docker Desktop** (for integration tests with TestContainers)
- ✅ **Git** for version control

---

## Step 1: Create Solution and Projects

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

# Create Controllers API project (Web API)
dotnet new webapi -n QuestionRandomizer.Api.Controllers -o src/QuestionRandomizer.Api.Controllers
dotnet sln add src/QuestionRandomizer.Api.Controllers

# Create Minimal API project (Web API)
dotnet new webapi -n QuestionRandomizer.Api.MinimalApi -o src/QuestionRandomizer.Api.MinimalApi
dotnet sln add src/QuestionRandomizer.Api.MinimalApi

# Create Test projects
dotnet new xunit -n QuestionRandomizer.UnitTests -o tests/QuestionRandomizer.UnitTests
dotnet sln add tests/QuestionRandomizer.UnitTests

dotnet new xunit -n QuestionRandomizer.IntegrationTests.Controllers -o tests/QuestionRandomizer.IntegrationTests.Controllers
dotnet sln add tests/QuestionRandomizer.IntegrationTests.Controllers

dotnet new xunit -n QuestionRandomizer.E2ETests -o tests/QuestionRandomizer.E2ETests
dotnet sln add tests/QuestionRandomizer.E2ETests
```

---

## Step 2: Add Project References

```bash
# Application depends on Domain
dotnet add src/QuestionRandomizer.Application reference src/QuestionRandomizer.Domain

# Infrastructure depends on Domain and Application
dotnet add src/QuestionRandomizer.Infrastructure reference src/QuestionRandomizer.Domain
dotnet add src/QuestionRandomizer.Infrastructure reference src/QuestionRandomizer.Application

# Controllers API depends on Application and Infrastructure
dotnet add src/QuestionRandomizer.Api.Controllers reference src/QuestionRandomizer.Application
dotnet add src/QuestionRandomizer.Api.Controllers reference src/QuestionRandomizer.Infrastructure

# Minimal API depends on Application and Infrastructure
dotnet add src/QuestionRandomizer.Api.MinimalApi reference src/QuestionRandomizer.Application
dotnet add src/QuestionRandomizer.Api.MinimalApi reference src/QuestionRandomizer.Infrastructure

# Unit Tests reference Application and Domain
dotnet add tests/QuestionRandomizer.UnitTests reference src/QuestionRandomizer.Application
dotnet add tests/QuestionRandomizer.UnitTests reference src/QuestionRandomizer.Domain

# Integration Tests reference Controllers API
dotnet add tests/QuestionRandomizer.IntegrationTests.Controllers reference src/QuestionRandomizer.Api.Controllers

# E2E Tests reference Controllers API
dotnet add tests/QuestionRandomizer.E2ETests reference src/QuestionRandomizer.Api.Controllers
```

---

## Step 3: Install NuGet Packages

### Domain Project (No external dependencies!)
```bash
# Domain has NO external dependencies - keep it pure!
```

### Application Project
```bash
cd src/QuestionRandomizer.Application
dotnet add package MediatR --version 12.4.1
dotnet add package FluentValidation --version 11.10.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.10.0
dotnet add package Microsoft.Extensions.Logging.Abstractions
cd ../..
```

### Infrastructure Project
```bash
cd src/QuestionRandomizer.Infrastructure
dotnet add package FirebaseAdmin --version 3.0.1
dotnet add package Google.Cloud.Firestore --version 3.8.0
dotnet add package Microsoft.Extensions.Http.Polly --version 10.0.0
dotnet add package Polly --version 8.4.2
dotnet add package Microsoft.AspNetCore.Http
cd ../..
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

### Unit Tests Project
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

1. ✅ **Read [CLAUDE.md](../CLAUDE.md)** - Project overview and architecture
2. ✅ **Read [ARCHITECTURE.md](../ARCHITECTURE.md)** - Detailed system architecture
3. ✅ **Read [CODE-TEMPLATES.md](./CODE-TEMPLATES.md)** - Code patterns and examples
4. ✅ **Read [CONFIGURATION.md](./CONFIGURATION.md)** - Configuration details
5. ✅ **Read [DUAL-API-GUIDE.md](./DUAL-API-GUIDE.md)** - Controllers vs Minimal API
6. ✅ **Start implementing** - Follow phase checklist in CLAUDE.md

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
