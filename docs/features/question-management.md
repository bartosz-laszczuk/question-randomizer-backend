# Feature: Question management

## Purpose
Own the CRUD lifecycle of interview questions — the core data every other capability builds on.

## Actors
Authenticated user (owner). Handled by the Questions module.

## Behavior
- Create / read / update / delete questions via CQRS commands/queries (MediatR).
- A question carries an English answer (`answer`) and Polish answer (`answerPl`), optional
  category/qualification references (id + denormalized name), `tags`, and an `isActive` flag.
- **Delete is soft** — sets `isActive=false` rather than removing the document.
- Supports **batch** create/update (`POST`/`PUT /api/questions/batch`) and **bulk reference
  removal** — clearing a category or qualification from all the user's questions
  (`DELETE /api/questions/category/{categoryId}` and `/qualification/{qualificationId}`), used by the
  cascade when a category/qualification is deleted.
- All reads and writes are scoped to the caller's `userId` (via `ICurrentUserService`).

## Acceptance criteria
- **Given** an authenticated user, **when** they create a question with the required fields
  (`question`, `answer`, `answerPl`, `isActive`, `userId`), **then** it is persisted to `questions`
  with their `userId`.
- **Given** a question owned by another user, **when** the caller requests/updates/deletes it,
  **then** the operation is denied/treated as not found.
- **Given** a delete, **then** the document remains but `isActive` becomes false (soft delete).
- **Given** a category or qualification is deleted, **then** the corresponding reference on the
  user's questions is cleared (see [category-management](category-management.md),
  [qualification-management](qualification-management.md)); the questions are not deleted.
- **Given** either API (5000 or 5001), **then** behavior and responses are identical.

## Data touched
`questions` (primary); references `categories`, `qualifications`. Shapes: [`schema.json`](../schema.json).

## Out of scope
Randomization/selection (see [randomization](randomization.md)); agent-driven edits (see [ai-agent](ai-agent.md)).
