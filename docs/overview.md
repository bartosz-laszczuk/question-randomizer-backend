# Overview — Question Randomizer Backend

## What this is

The **backend API** for the Question Randomizer system — a .NET 10 (C#) service that owns the data,
the business logic, authentication/authorization, and an **integrated AI agent**. It is the
system's data authority: the Angular frontend consumes this API's contract, and this service is the
producer of that contract.

## Who it's for

- **The frontend** (`worse-and-pricier`), which calls this API for AI-agent features and (per the
  drift note below) is the intended single entry point to the data.
- **Interview candidates / interviewers** (end users), indirectly — every request is scoped to the
  authenticated Firebase user, and all data is isolated per `userId`.

## The problem it solves

Provide a single, secure, well-structured place for:
- **CRUD** over questions, categories, qualifications, conversations, and randomization sessions.
- **Randomization** logic and session tracking (used/postponed/selected questions).
- An **autonomous AI agent** that can manage a user's data through natural language, using 15
  specialized tools with real-time streaming.
- **Authentication** (Firebase ID tokens) and **authorization** (roles/policies), enforcing that a
  user can only touch their own data.

## Scope of this repository

In scope:
- The REST API (two implementations — Controllers and Minimal API — of the same contract).
- Four business modules (Questions, Conversations, Randomization, Agent) as vertical slices.
- Direct Firestore access via the Firebase Admin SDK.
- The AI agent, its tools, and streaming execution.

Out of scope (owned elsewhere):
- The user interface → `worse-and-pricier`.
- System-wide, cross-service architecture → the `question-randomizer` coordination repo.

## Success criteria (product-level)

- An authenticated user can CRUD their questions, categories, and qualifications, and never see or
  modify another user's data.
- Deleting a category/qualification clears its references from questions (cross-module, via a
  domain event) without deleting the questions.
- A user's randomization **session state** (selected categories, used and postponed questions,
  current question, status) is persisted per user and survives across requests; deleting a category
  clears it from the session. (The random-selection algorithm itself runs client-side.)
- A user can drive data management conversationally; the agent streams progress (thinking, tool
  calls, results) and persists conversation history.
- The same behavior is available identically on both the Controllers API (port 5000) and the
  Minimal API (port 5001).

## Tech stack

Owned by [`architecture.md`](architecture.md); not restated here.

## Known drift (flagged for reconciliation)

- The coordination repo and this repo's older docs state "Frontend → Backend API → Firestore" as
  the only path. In practice the **frontend also reads/writes Firestore directly** for core CRUD and
  uses this backend only for the AI agent. Recorded in the frontend's specs too; the system-level
  intent and the code disagree.
- Legacy Clean-Architecture projects (`QuestionRandomizer.Application/Domain/Infrastructure`) still
  exist under `src/` although the migration was described as complete. See [`architecture.md`](architecture.md).
