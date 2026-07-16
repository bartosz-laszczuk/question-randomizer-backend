# Question Randomizer Backend

.NET 10 backend API for the Question Randomizer system — data, business logic, auth, and an
integrated AI agent. Two API implementations (Controllers @ 5000, Minimal @ 5001) of one contract.

- **What it is, who it's for, why** → [`docs/overview.md`](docs/overview.md)
- **How it's built (architecture & tech decisions)** → [`docs/architecture.md`](docs/architecture.md)
- **What it does (feature specs)** → [`docs/features/`](docs/features/)
- **API contract** → [`docs/api.md`](docs/api.md) · **Data model** → [`docs/schema.json`](docs/schema.json)
- **Working in this repo as an agent** → [`CLAUDE.md`](CLAUDE.md)

This README covers only running the service. Everything descriptive lives in the spec under
[`docs/`](docs/); supporting guides (setup, config, deployment, testing, auth, agent tools) are in
[`docs/guides/`](docs/guides/).

## Prerequisites

- .NET 10 SDK (`dotnet --version` → 10.x)
- A Firebase project (Firestore + Auth). Save the service-account key as
  `firebase-dev-credentials.json` and reference it from `appsettings.Development.json`
  (see [`docs/guides/configuration.md`](docs/guides/configuration.md)). **Never commit credentials.**
- An Anthropic API key for the AI agent (`Anthropic:*` config).
- Docker Desktop (optional; for integration tests / compose).

## Run

```bash
dotnet build

# Controllers API → http://localhost:5000/swagger
cd src/QuestionRandomizer.Api.Controllers && dotnet run

# Minimal API → http://localhost:5001/swagger   (separate terminal)
cd src/QuestionRandomizer.Api.MinimalApi && dotnet run
```

Or both via Docker: `cp .env.example .env` then `docker-compose up -d`
(health: `curl http://localhost:5000/health`).

## Test

```bash
dotnet test                                              # all
dotnet test tests/QuestionRandomizer.Modules.Questions.Tests
dotnet test /p:CollectCoverage=true                      # with coverage
```

Testing strategy → [`docs/guides/testing.md`](docs/guides/testing.md).
Setup, deployment, security → [`docs/guides/`](docs/guides/).
