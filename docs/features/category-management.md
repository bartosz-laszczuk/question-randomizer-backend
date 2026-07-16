# Feature: Category management

## Purpose
Manage user-scoped categories used to organize and to scope randomization, and keep questions
consistent when a category is removed.

## Actors
Authenticated user (owner). Questions module.

## Behavior
- Create (single or **batch**), read, update, delete categories (a category is a user-scoped `name`).
- Deleting a category publishes a **`CategoryDeletedEvent`** (domain event). Handlers in the
  Questions and Randomization modules react to clear the reference from affected data — modules do
  not call each other directly.

## Acceptance criteria
- **Given** an authenticated user, **when** they create a category (or a batch), **then** each is
  persisted to `categories` with their `userId`.
- **Given** a category referenced by questions, **when** the user deletes it, **then** a
  `CategoryDeletedEvent` is published **and** every one of that user's questions with that
  `categoryId` has the reference cleared, while the questions themselves remain.
- **Given** a category owned by another user, **when** the caller acts on it, **then** it is
  treated as not found.
- **Given** either API, **then** behavior is identical.

## Data touched
`categories` (primary); cascades (via event) to `questions.categoryId` and randomization session
state. Shapes: [`schema.json`](../schema.json).

## Out of scope
Question CRUD (see [question-management](question-management.md)).
