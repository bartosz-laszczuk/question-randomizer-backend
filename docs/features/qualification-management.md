# Feature: Qualification management

## Purpose
Manage user-scoped qualification levels (e.g. "Junior", "Senior") and keep questions consistent when
one is removed.

## Actors
Authenticated user (owner). Questions module.

## Behavior
- Create (single or **batch**), read, update, delete qualifications (a user-scoped `name`).
- Deleting a qualification publishes a qualification-deleted domain event; handlers clear the
  reference from affected questions.

## Acceptance criteria
- **Given** an authenticated user, **when** they create a qualification (or batch), **then** each is
  persisted to `qualifications` with their `userId`.
- **Given** a qualification referenced by questions, **when** the user deletes it, **then** the
  deleted event is published **and** every one of that user's questions with that
  `qualificationId` has the reference cleared, while the questions remain.
- **Given** a qualification owned by another user, **then** the operation is treated as not found.
- **Given** either API, **then** behavior is identical.

## Data touched
`qualifications` (primary); cascades to `questions.qualificationId`. Shapes: [`schema.json`](../schema.json).

## Out of scope
Question CRUD (see [question-management](question-management.md)).
