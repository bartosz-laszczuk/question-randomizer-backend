# API — the contract this backend produces

This backend is the **producer** of the system's API contract. Both API implementations
(Controllers @ 5000, Minimal @ 5001) expose the **same** endpoints; every change must land in both.
Data shapes are in [`schema.json`](schema.json); the Controllers-vs-Minimal comparison is in
[`guides/DUAL-API-GUIDE.md`](guides/DUAL-API-GUIDE.md).

## Canonical machine-readable spec

The authoritative OpenAPI document is **generated at runtime by Swashbuckle** and served at
`/swagger` (JSON at `/swagger/v1/swagger.json`) on each API. That generated file is the single
source of truth the **frontend should generate its client from** (see the cross-repo contract
strategy in the frontend's `docs/api.md`).

> ⚠️ **TODO.** There is no committed `openapi.yaml`. For the frontend to generate a client with a
> CI freshness check, export the Swashbuckle output to a versioned artifact (committed
> `openapi.yaml` or a published package). Until then this document is the human-readable contract.

## Authentication

All endpoints (except `/health`) require `Authorization: Bearer <Firebase ID token>`. The verified
`userId` scopes every operation. `/api/admin/*` additionally requires the `AdminPolicy`.

## Endpoints

### Questions
| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/questions` | List the user's questions |
| GET | `/api/questions/{id}` | Get one |
| POST | `/api/questions` | Create |
| PUT | `/api/questions/{id}` | Update |
| DELETE | `/api/questions/{id}` | Soft delete (`isActive=false`) |
| POST | `/api/questions/batch` | Create many |
| PUT | `/api/questions/batch` | Update many |
| DELETE | `/api/questions/category/{categoryId}` | Clear a category from all the user's questions |
| DELETE | `/api/questions/qualification/{qualificationId}` | Clear a qualification from all the user's questions |

### Categories
| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/categories` · `/api/categories/{id}` | List / get |
| POST | `/api/categories` · `/api/categories/batch` | Create one / many |
| PUT | `/api/categories/{id}` | Update |
| DELETE | `/api/categories/{id}` | Delete (emits `CategoryDeletedEvent`) |

### Qualifications
| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/qualifications` · `/api/qualifications/{id}` | List / get |
| POST | `/api/qualifications` · `/api/qualifications/batch` | Create one / many |
| PUT | `/api/qualifications/{id}` | Update |
| DELETE | `/api/qualifications/{id}` | Delete (emits qualification-deleted event) |

### Conversations
| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/conversations` · `/api/conversations/{id}` | List / get |
| POST | `/api/conversations` | Create |
| GET | `/api/conversations/{id}/messages` | List messages |
| POST | `/api/conversations/{id}/messages` | Add a message |
| POST | `/api/conversations/{id}/update-timestamp` | Touch `updatedAt` (there is no PUT update) |
| DELETE | `/api/conversations/{id}` | Delete (hard) |

### Randomization (`/api/randomizations`)
This is a **session-state resource**, not a "randomize" action — the drawing/selection logic runs
client-side; the backend persists the session and its sub-collections.

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/randomizations` | Get the user's session |
| POST | `/api/randomizations` | Create a session |
| PUT | `/api/randomizations/{id}` | Update session (showAnswer, status, currentQuestionId) |
| POST | `/api/randomizations/{id}/clear-current-question` | Clear `currentQuestionId` |
| GET · POST | `/api/randomizations/{id}/selected-categories` | List / add selected category |
| DELETE | `/api/randomizations/{id}/selected-categories/{categoryId}` | Remove selected category |
| GET · POST | `/api/randomizations/{id}/used-questions` | List / add used question |
| DELETE | `/api/randomizations/{id}/used-questions/{questionId}` | Remove used question |
| PUT | `/api/randomizations/{id}/used-questions/category` | Update used-question category |
| GET · POST | `/api/randomizations/{id}/postponed-questions` | List / add postponed question |
| DELETE | `/api/randomizations/{id}/postponed-questions/{questionId}` | Remove postponed question |
| PUT | `/api/randomizations/{id}/postponed-questions/{questionId}/timestamp` | Update postponed timestamp |

### Agent
| Method | Path | Purpose |
|--------|------|---------|
| POST | `/api/agent/execute` | Execute an AI task with SSE streaming |

Request: `{ "task": string, "conversationId"?: string }`. Response: an SSE stream of events typed
`started` · `thinking` · `text_chunk` · `tool_call` · `tool_result` · `completed` · `error`.
Behavior: [`features/ai-agent.md`](features/ai-agent.md).

### Admin (requires `AdminPolicy`)
| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/admin/me` | Admin identity info |
| GET | `/api/admin/health` | System health (admin view) |
| GET | `/api/admin/users` | List users |
| GET | `/api/admin/analytics` | System analytics |

### Health
| Method | Path | Purpose |
|--------|------|---------|
| GET | `/health` | Liveness (unauthenticated) |
