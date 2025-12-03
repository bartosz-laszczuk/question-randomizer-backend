# Question Randomizer Backend

> Production-ready .NET 10 backend API with dual implementation (Controllers + Minimal API)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14-239120?logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Tests](https://img.shields.io/badge/Tests-453%20Passing-success)](./docs/TESTING.md)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## 🎯 Overview

A production-ready backend API for the Question Randomizer interview preparation application. Features a **dual API implementation** demonstrating Clean Architecture principles with both Controllers and Minimal API approaches sharing the same business logic.

### Key Features

- ✅ **Dual API Implementation** - Controllers (Port 5000) + Minimal API (Port 5001)
- ✅ **Clean Architecture** - Clear separation of concerns
- ✅ **CQRS Pattern** - Command/Query separation with MediatR
- ✅ **Firebase Integration** - Firestore database + Authentication
- ✅ **AI Agent Integration** - Orchestration of autonomous AI tasks
- ✅ **Comprehensive Testing** - 453 tests (352 unit + 101 integration)
- ✅ **Production Ready** - Docker, CI/CD, security audit, deployment docs

---

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)
- Firebase project with Firestore enabled

### Run Locally

```bash
# Clone repository
git clone https://github.com/your-org/question-randomizer-backend.git
cd question-randomizer-backend

# Configure Firebase (see Configuration section)
# Add firebase-dev-credentials.json to project root

# Run Controllers API (Port 5000)
cd src/QuestionRandomizer.Api.Controllers
dotnet run
# Swagger UI: http://localhost:5000/swagger

# Run Minimal API (Port 5001) - in separate terminal
cd src/QuestionRandomizer.Api.MinimalApi
dotnet run
# Swagger UI: http://localhost:5001/swagger
```

### Run with Docker

```bash
# Configure environment
cp .env.example .env
# Edit .env with your Firebase project ID

# Start both APIs
docker-compose up -d

# Verify health
curl http://localhost:5000/health
curl http://localhost:5001/health
```

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| **[CLAUDE.md](./CLAUDE.md)** | Developer guide with architecture and project overview |
| **[TESTING.md](./docs/TESTING.md)** | Testing strategy and 453-test suite details |
| **[DEPLOYMENT.md](./docs/DEPLOYMENT.md)** | Deployment guide (Docker, Azure, AWS, K8s) |
| **[SECURITY-AUDIT.md](./docs/SECURITY-AUDIT.md)** | Security checklist and best practices |
| **[DUAL-API-GUIDE.md](./docs/DUAL-API-GUIDE.md)** | Controllers vs Minimal API comparison |
| **[CODE-TEMPLATES.md](./docs/CODE-TEMPLATES.md)** | Code patterns and templates |
| **[CONFIGURATION.md](./docs/CONFIGURATION.md)** | Configuration details and examples |
| **[SETUP-GUIDE.md](./docs/SETUP-GUIDE.md)** | Step-by-step setup instructions |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Angular Frontend                        │
│                  (Port 4200)                            │
└────────────────────┬────────────────────────────────────┘
                     ↓ HTTPS
          ┌──────────┴──────────┐
          ↓                     ↓
┌──────────────────┐  ┌──────────────────┐
│  Controllers API  │  │   Minimal API    │
│    (Port 5000)   │  │   (Port 5001)    │
└────────┬─────────┘  └────────┬──────────┘
         │                     │
         └──────────┬──────────┘
                    ↓
      ┌─────────────┴─────────────┐
      │  Shared Business Logic    │
      │  (Domain + Application)   │
      └─────────────┬─────────────┘
                    ↓
         ┌──────────┴──────────┐
         ↓                     ↓
  ┌──────────────┐    ┌──────────────┐
  │   Firebase   │    │ Agent Service│
  │  Firestore   │    │ (Port 3002)  │
  └──────────────┘    └──────────────┘
```

### Clean Architecture Layers

```
src/
├── QuestionRandomizer.Domain/          # Entities, Interfaces
├── QuestionRandomizer.Application/     # CQRS, Handlers, Validators
├── QuestionRandomizer.Infrastructure/  # Firebase, External Services
├── QuestionRandomizer.Api.Controllers/ # Controllers API (5000)
└── QuestionRandomizer.Api.MinimalApi/  # Minimal API (5001)
```

---

## 🧪 Testing

**Total: 453 Tests Passing (100% Pass Rate)**

- **352 Unit Tests** - All handlers and validators
- **50 Integration Tests** - Controllers API endpoints
- **51 Integration Tests** - Minimal API endpoints
- **24 E2E Tests** - Complete workflows (requires Firebase Emulator)

```bash
# Run all tests
dotnet test

# Run specific test suite
dotnet test tests/QuestionRandomizer.UnitTests
dotnet test tests/QuestionRandomizer.IntegrationTests.Controllers
dotnet test tests/QuestionRandomizer.IntegrationTests.MinimalApi
```

See **[TESTING.md](./docs/TESTING.md)** for detailed test documentation.

---

## 📦 Project Structure

```
question-randomizer-backend/
├── src/                          # Source code (5 projects)
│   ├── QuestionRandomizer.Domain/
│   ├── QuestionRandomizer.Application/
│   ├── QuestionRandomizer.Infrastructure/
│   ├── QuestionRandomizer.Api.Controllers/
│   └── QuestionRandomizer.Api.MinimalApi/
├── tests/                        # Tests (4 projects)
│   ├── QuestionRandomizer.UnitTests/
│   ├── QuestionRandomizer.IntegrationTests.Controllers/
│   ├── QuestionRandomizer.IntegrationTests.MinimalApi/
│   └── QuestionRandomizer.E2ETests/
├── docs/                         # Documentation
├── .github/workflows/            # CI/CD pipelines
├── docker-compose.yml            # Docker orchestration
└── QuestionRandomizer.sln        # Solution file
```

---

## 🔧 Configuration

### Firebase Setup

1. Get service account key from [Firebase Console](https://console.firebase.google.com/)
2. Save as `firebase-dev-credentials.json`
3. Update `appsettings.Development.json`:

```json
{
  "Firebase": {
    "ProjectId": "your-project-id",
    "CredentialsPath": "firebase-dev-credentials.json"
  }
}
```

**⚠️ Important:** Never commit Firebase credentials to Git!

### Environment Variables

See **[CONFIGURATION.md](./docs/CONFIGURATION.md)** for complete configuration details.

---

## 🚢 Deployment

### Docker Deployment

```bash
# Build images
docker build -t question-randomizer-controllers:latest \
  -f src/QuestionRandomizer.Api.Controllers/Dockerfile .

# Deploy with docker-compose
docker-compose up -d
```

### Cloud Deployment

- **Azure App Service** - See [DEPLOYMENT.md](./docs/DEPLOYMENT.md#azure-app-service)
- **AWS Elastic Beanstalk** - See [DEPLOYMENT.md](./docs/DEPLOYMENT.md#aws-elastic-beanstalk)
- **Kubernetes** - See [DEPLOYMENT.md](./docs/DEPLOYMENT.md#kubernetes-k8s)

---

## 🔒 Security

Comprehensive security checklist covering:
- Authentication & Authorization
- Secrets Management
- Input Validation
- CORS Configuration
- HTTPS & Transport Security
- Docker Security
- Dependency Management

See **[SECURITY-AUDIT.md](./docs/SECURITY-AUDIT.md)** for the complete checklist.

---

## 🛣️ API Endpoints

### Questions
```
GET    /api/questions             # List all questions
GET    /api/questions/{id}        # Get question by ID
POST   /api/questions             # Create question
PUT    /api/questions/{id}        # Update question
DELETE /api/questions/{id}        # Delete question
```

### Categories, Qualifications, Conversations, Randomization
See **[CLAUDE.md](./CLAUDE.md#api-endpoints)** for complete API specification.

---

## 📊 Technology Stack

| Layer | Technology |
|-------|------------|
| **Framework** | .NET 10, C# 14 |
| **Architecture** | Clean Architecture, CQRS |
| **Patterns** | MediatR, Repository Pattern |
| **Validation** | FluentValidation |
| **Database** | Firebase Firestore |
| **Authentication** | Firebase Auth |
| **API Styles** | Controllers + Minimal API |
| **Testing** | xUnit, Moq, FluentAssertions |
| **Containerization** | Docker, Docker Compose |
| **CI/CD** | GitHub Actions |

---

## 🎓 Learning Resources

This project demonstrates:
- **Clean Architecture** - Dependency inversion and separation of concerns
- **CQRS Pattern** - Command/Query segregation with MediatR
- **Dual API Implementation** - Controllers vs Minimal API comparison
- **Comprehensive Testing** - Unit, Integration, E2E test strategies
- **Production Readiness** - Docker, CI/CD, security, deployment

Perfect for learning modern .NET development best practices!

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

See **[CODE-TEMPLATES.md](./docs/CODE-TEMPLATES.md)** for code patterns and conventions.

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) by Robert C. Martin
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html) by Martin Fowler
- [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard
- [Firebase](https://firebase.google.com/) by Google

---

## 📞 Support

- **Documentation:** [/docs](./docs)
- **Issues:** [GitHub Issues](https://github.com/your-org/question-randomizer-backend/issues)
- **Discussions:** [GitHub Discussions](https://github.com/your-org/question-randomizer-backend/discussions)

---

**Built with ❤️ using .NET 10**

**Status:** ✅ Production Ready | **Tests:** 453 Passing | **Last Updated:** 2025-11-30
