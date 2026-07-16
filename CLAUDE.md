# CLAUDE.md

Operating manual for AI agents working in this repository. This file contains **only** agent
directives and a map to the single-source-of-truth documents. It does **not** restate product,
architecture, API, or how-to-run content — those live once, in the places below.

## Documentation map (single source of truth)

| Topic | Owner |
|-------|-------|
| What/why/who, success criteria | [`docs/overview.md`](docs/overview.md) |
| Feature behavior & acceptance criteria | [`docs/features/`](docs/features/) |
| Architecture & tech decisions | [`docs/architecture.md`](docs/architecture.md) |
| API contract (endpoints, streaming) | [`docs/api.md`](docs/api.md) |
| Firestore data shapes | [`docs/schema.json`](docs/schema.json) |
| Authorization model | [`docs/guides/authorization.md`](docs/guides/authorization.md) |
| Agent tools (reference / dev / examples) | [`docs/guides/agent-tools-reference.md`](docs/guides/agent-tools-reference.md), [`docs/guides/agent-tool-development.md`](docs/guides/agent-tool-development.md), [`docs/guides/agent-examples.md`](docs/guides/agent-examples.md) |
| Agent module dev guide | [`src/Modules/QuestionRandomizer.Modules.Agent/CLAUDE.md`](src/Modules/QuestionRandomizer.Modules.Agent/CLAUDE.md) |
| Code patterns / templates | [`docs/guides/code-templates.md`](docs/guides/code-templates.md) |
| Controllers vs Minimal API | [`docs/guides/dual-api-guide.md`](docs/guides/dual-api-guide.md) |
| Setup / configuration / deployment | [`docs/guides/setup-guide.md`](docs/guides/setup-guide.md), [`docs/guides/configuration.md`](docs/guides/configuration.md), [`docs/guides/deployment.md`](docs/guides/deployment.md) |
| Testing strategy | [`docs/guides/testing.md`](docs/guides/testing.md) |
| Security audit / migration history | [`docs/guides/security-audit.md`](docs/guides/security-audit.md), [`docs/guides/migration-summary.md`](docs/guides/migration-summary.md) |
| Run / build / test commands | [`README.md`](README.md) |

## Spec-Driven Development workflow

Spec-first. The spec drives the code.

- **New feature:** write/extend its `docs/features/<name>.md` (purpose, behavior, Given/When/Then
  acceptance criteria, data touched) **before** implementing. Tests verify those criteria.
- **Changing behavior:** update the owning spec in the same change; keep `docs/api.md` and
  `docs/schema.json` in step with any contract/data change.
- **Baseline caveat:** the specs under `docs/` were reverse-derived from the implementation on
  2026-07-16; items marked **⚠️ DRIFT** are known spec-vs-code discrepancies to reconcile.
- **No duplication:** every fact lives in exactly one owner above; everywhere else links.

## Mandatory engineering rules

1. **Modular monolith:** add a feature inside its module (Questions, Conversations, Randomization,
   Agent). Modules **never** reference each other directly — cross-module integration is via
   **domain events** only (e.g. `CategoryDeletedEvent`).
2. **Dual API:** every endpoint change must be applied to **both** `Api.Controllers` (5000) **and**
   `Api.MinimalApi` (5001); they must stay functionally identical.
3. **CQRS:** commands/queries via MediatR handlers; validate with FluentValidation behaviors.
4. **Security:** resolve identity only via `ICurrentUserService`; **never trust a client-supplied
   `userId`**; verify ownership before every read/update/delete; never commit Firebase/Anthropic
   credentials.
5. **Patterns:** follow [`docs/guides/code-templates.md`](docs/guides/code-templates.md); write tests
   for new behavior (module + integration on both APIs).

## Known drift to reconcile

- Legacy Clean-Architecture projects (`QuestionRandomizer.Application/Domain/Infrastructure`) still
  exist under `src/` despite the modular-monolith migration being described as complete.
- Cosmetic dual-API difference: Controllers' Swagger reports PascalCase paths, Minimal uses
  lowercase (routing is case-insensitive). Committed `openapi.json` uses the Minimal (lowercase) form.
- Firestore field mapping: C# entity property names (e.g. `Question.QuestionText`) vs the stored
  field names the frontend uses (`question`) — verify the `ConvertTo<T>` mapping. See
  [`docs/architecture.md`](docs/architecture.md) and [`docs/schema.json`](docs/schema.json).
- "Frontend → Backend → Firestore" is the stated intent, but the frontend also accesses Firestore
  directly today. See [`docs/overview.md`](docs/overview.md).
