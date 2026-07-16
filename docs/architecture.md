# Architecture — Question Randomizer Backend

How this service is structured and the technology decisions behind it. Product context is in
[`overview.md`](overview.md); behavior is in [`features/`](features/); data shapes are in
[`schema.json`](schema.json); the API contract is in [`api.md`](api.md).

## Tech stack

| Concern | Choice |
|---------|--------|
| Runtime / language | .NET 10, C# 14, ASP.NET Core 10 |
| Architecture | Modular monolith (vertical slices) + CQRS |
| Mediation | MediatR (commands/queries + domain-event notifications) |
| Validation | FluentValidation |
| Data | Firebase Firestore via `Google.Cloud.Firestore` (Admin SDK) |
| Auth | Firebase Authentication (ID token bearer) + policy-based authorization |
| AI | Anthropic Claude SDK — model `claude-sonnet-4-5-20250929` (configurable via `Anthropic:Model`) |
| Resilience / observability | Polly, OpenTelemetry |
| API docs | Swashbuckle (Swagger / OpenAPI) |
| Testing | xUnit, Moq, FluentAssertions, Bogus, TestContainers |

## Modular monolith

Business capability is organized into **vertical slices**, each with its own `Domain`,
`Application`, and `Infrastructure`:

- **Questions** — questions, categories, qualifications (owns the `CategoryDeleted`/`QualificationDeleted` domain events).
- **Conversations** — conversations and messages (AI chat history).
- **Randomization** — randomization sessions and session tracking (subscribes to Questions' events).
- **Agent** — the integrated AI agent: 15 tools, Claude executor, streaming.

A **SharedKernel** project holds cross-cutting concerns: domain-event infrastructure, base
entities, common interfaces (`ICurrentUserService`), Firebase setup, and authorization primitives.

### Cross-module communication — domain events only

Modules never reference each other directly. They integrate through **domain events** dispatched by
MediatR (`INotification`). Canonical example: Questions publishes `CategoryDeletedEvent`;
Randomization (and the Questions event handler) react to clear references — see
[`features/category-management.md`](features/category-management.md).

### CQRS with MediatR

```
API (Controller / Minimal endpoint) → MediatR → Command/Query Handler → Repository → Firestore
                                          └── publishes Domain Events → cross-module handlers
```

Commands/queries are records implementing `IRequest<T>`; handlers implement `IRequestHandler<,>`.
Validation runs as a FluentValidation pipeline behavior. Handlers resolve the caller via
`ICurrentUserService` and scope every operation by `userId`.

## Dual API (deliberate)

Two presentation projects expose the **same** contract over the same modules:

- **`QuestionRandomizer.Api.Controllers`** — traditional `[ApiController]` classes, port **5000**.
- **`QuestionRandomizer.Api.MinimalApi`** — Minimal API endpoint groups, port **5001**.

They are functionally identical; the split is a learning/comparison exercise showing the
presentation layer is independent of the business architecture. Comparison details:
[`guides/DUAL-API-GUIDE.md`](guides/DUAL-API-GUIDE.md). **Any endpoint change must be applied to both.**

## AI agent module

The agent runs **in-process**. `AgentExecutor` drives a Claude model through an agentic loop,
invoking tools from a `ToolRegistry`. The 15 tools are grouped as:

- **DataRetrieval** (6): get questions, get by id, search, uncategorized, categories, qualifications.
- **DataModification** (7): create question/category/qualification, update question, update question
  category, batch-update questions, delete question.
- **DataAnalysis** (2): find duplicate questions, analyze question difficulty.

Execution streams Server-Sent Events (`started`, `thinking`, `text_chunk`, `tool_call`,
`tool_result`, `completed`, `error`) with a configurable execution timeout (default 120 s /
2 minutes, `AgentConfiguration.TimeoutSeconds`) and conversation-context memory.
Detailed agent dev docs live with the module: `src/Modules/QuestionRandomizer.Modules.Agent/CLAUDE.md`,
plus [`guides/AGENT-TOOLS-REFERENCE.md`](guides/AGENT-TOOLS-REFERENCE.md),
[`guides/AGENT-TOOL-DEVELOPMENT.md`](guides/AGENT-TOOL-DEVELOPMENT.md),
[`guides/AGENT-EXAMPLES.md`](guides/AGENT-EXAMPLES.md).

## Data access

All modules read/write Firestore directly through the Firebase Admin SDK (`FirestoreDb`).
Collection names are centralized in `SharedKernel/Infrastructure/Firebase/FirestoreCollections.cs`;
the stored shapes are specified in [`schema.json`](schema.json). Repositories verify ownership
(`userId`) on every read/update/delete.

> ⚠️ **DRIFT.** Domain entities (e.g. `Question.QuestionText`) are converted with Firestore
> `ConvertTo<T>` but carry **no** `[FirestoreData]`/`[FirestoreProperty]` attributes, while the
> frontend reads fields like `question`. The property-name→stored-field mapping needs verification;
> `schema.json` documents the **stored** wire shape (the interop contract), flagging where C#
> property names diverge.

## Authentication & authorization

Firebase ID token (bearer) authenticates each request; `ICurrentUserService` exposes the verified
`userId`. Authorization is policy-based (e.g. `AdminPolicy` guards `/api/admin/*`). Full model:
[`guides/AUTHORIZATION.md`](guides/AUTHORIZATION.md); behavior:
[`features/authentication-and-authorization.md`](features/authentication-and-authorization.md).

## Known structural drift

The migration to modular monolith is described as complete, but legacy Clean-Architecture projects
`QuestionRandomizer.Application`, `QuestionRandomizer.Domain`, and `QuestionRandomizer.Infrastructure`
still exist under `src/`. They should be confirmed dead and removed, or documented as still in use.

## Cross-cutting references

- Configuration & ports → [`guides/CONFIGURATION.md`](guides/CONFIGURATION.md), [`guides/SETUP-GUIDE.md`](guides/SETUP-GUIDE.md)
- Deployment → [`guides/DEPLOYMENT.md`](guides/DEPLOYMENT.md)
- Testing strategy → [`guides/TESTING.md`](guides/TESTING.md)
- Code patterns/templates → [`guides/CODE-TEMPLATES.md`](guides/CODE-TEMPLATES.md)
- Migration history → [`guides/MIGRATION-SUMMARY.md`](guides/MIGRATION-SUMMARY.md)
