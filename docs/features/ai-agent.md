# Feature: AI agent

## Purpose
Let a user manage their data through natural language. The integrated agent plans and executes
tasks autonomously using specialized tools, streaming its progress in real time.

## Actors
Authenticated user. Agent module (in-process Claude executor + tool registry).

## Behavior
- `POST /api/agent/execute` with `{ task, conversationId? }` runs an agentic loop against a Claude
  model (`claude-sonnet-4-5-20250929`, configurable) and streams **Server-Sent Events**.
- The agent selects and invokes **15 tools** across three groups:
  - **DataRetrieval (6):** get questions, get by id, search, uncategorized, categories, qualifications.
  - **DataModification (7):** create question/category/qualification, update question, update
    question category, batch-update questions, delete question.
  - **DataAnalysis (2):** find duplicate questions, analyze question difficulty.
- Tools operate only on the authenticated user's data.
- With a `conversationId`, prior turns are loaded so the agent has **multi-turn memory**; exchanges
  are persisted (see [conversation-management](conversation-management.md)).
- Execution has a **configurable timeout** (default **120 s / 2 minutes**, `TimeoutSeconds`).

## Acceptance criteria
- **Given** an unauthenticated request, **then** it is rejected before execution.
- **Given** a task, **when** the agent runs, **then** the response is an SSE stream emitting, in
  order, `started` → (`thinking` | `tool_call` → `tool_result` | `text_chunk`)* → `completed`, and
  any failure emits `error`.
- **Given** a tool call, **then** it only reads/writes the caller's own data (scoped by `userId`).
- **Given** a `conversationId`, **then** prior messages are included as context and the new exchange
  is persisted to that conversation.
- **Given** a task exceeding the configured timeout (default 120 s), **then** it times out and emits `error`.
- **Given** either API, **then** behavior is identical.

## Data touched
Reads/writes `questions`, `categories`, `qualifications` via tools; `conversations`/`messages` for
memory. Shapes: [`schema.json`](../schema.json). Endpoint/stream contract: [`api.md`](../api.md).

## Dependencies
Anthropic API key configured (`Anthropic:*`). Detailed tool docs:
`src/Modules/QuestionRandomizer.Modules.Agent/CLAUDE.md`, [`guides/agent-tools-reference.md`](../guides/agent-tools-reference.md).

## Out of scope
Individual CRUD contracts (see the respective management features).
