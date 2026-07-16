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
| Authorization model | [`docs/guides/AUTHORIZATION.md`](docs/guides/AUTHORIZATION.md) |
| Agent tools (reference / dev / examples) | [`docs/guides/AGENT-TOOLS-REFERENCE.md`](docs/guides/AGENT-TOOLS-REFERENCE.md), [`docs/guides/AGENT-TOOL-DEVELOPMENT.md`](docs/guides/AGENT-TOOL-DEVELOPMENT.md), [`docs/guides/AGENT-EXAMPLES.md`](docs/guides/AGENT-EXAMPLES.md) |
| Agent module dev guide | [`src/Modules/QuestionRandomizer.Modules.Agent/CLAUDE.md`](src/Modules/QuestionRandomizer.Modules.Agent/CLAUDE.md) |
| Code patterns / templates | [`docs/guides/CODE-TEMPLATES.md`](docs/guides/CODE-TEMPLATES.md) |
| Controllers vs Minimal API | [`docs/guides/DUAL-API-GUIDE.md`](docs/guides/DUAL-API-GUIDE.md) |
| Setup / configuration / deployment | [`docs/guides/SETUP-GUIDE.md`](docs/guides/SETUP-GUIDE.md), [`docs/guides/CONFIGURATION.md`](docs/guides/CONFIGURATION.md), [`docs/guides/DEPLOYMENT.md`](docs/guides/DEPLOYMENT.md) |
| Testing strategy | [`docs/guides/TESTING.md`](docs/guides/TESTING.md) |
| Security audit / migration history | [`docs/guides/SECURITY-AUDIT.md`](docs/guides/SECURITY-AUDIT.md), [`docs/guides/MIGRATION-SUMMARY.md`](docs/guides/MIGRATION-SUMMARY.md) |
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
5. **Patterns:** follow [`docs/guides/CODE-TEMPLATES.md`](docs/guides/CODE-TEMPLATES.md); write tests
   for new behavior (module + integration on both APIs).

## Known drift to reconcile

- Legacy Clean-Architecture projects (`QuestionRandomizer.Application/Domain/Infrastructure`) still
  exist under `src/` despite the modular-monolith migration being described as complete.
- Firestore field mapping: C# entity property names (e.g. `Question.QuestionText`) vs the stored
  field names the frontend uses (`question`) — verify the `ConvertTo<T>` mapping. See
  [`docs/architecture.md`](docs/architecture.md) and [`docs/schema.json`](docs/schema.json).
- "Frontend → Backend → Firestore" is the stated intent, but the frontend also accesses Firestore
  directly today. See [`docs/overview.md`](docs/overview.md).
