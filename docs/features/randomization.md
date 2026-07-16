# Feature: Randomization (session state)

## Purpose
Persist a user's randomization **session state** — the current question, whether the answer is
shown, the session status, and the selected-categories / used-questions / postponed-questions
sub-collections — so a practice session survives across requests and devices.

> The **drawing/selection algorithm runs client-side** (the frontend). This backend is the
> **store** for session state, not the randomizer. There is no "randomize" endpoint.

## Actors
Authenticated user. Randomization module (subscribes to Questions' domain events).

## Behavior
- One session per user (`randomizations`): create it, read it, and update `showAnswer`, `status`
  (`Ongoing`/`Completed`), and `currentQuestionId`; clear the current question.
- Manage three per-session sub-collections, each add/list/remove:
  - **selected-categories** — categories scoping the session.
  - **used-questions** — questions already served (add/remove, and update a used question's category).
  - **postponed-questions** — deferred questions (add/remove, and update a postponed timestamp).
- React to `CategoryDeletedEvent` (cross-module domain event) to clear a deleted category from
  session state.
- All operations verify ownership by `userId`.

## Acceptance criteria
- **Given** an authenticated user, **when** they create a session, **then** it is persisted to
  `randomizations` with their `userId` and `status=Ongoing`.
- **Given** a session, **when** a used question is added, **then** it appears in the session's
  used-questions and is retrievable; removing it deletes it.
- **Given** a session, **when** a question is postponed, **then** it appears in postponed-questions
  with an updatable timestamp.
- **Given** the client updates the session, **then** `showAnswer`, `status`, and `currentQuestionId`
  persist; clear-current-question nulls `currentQuestionId`.
- **Given** a category referenced by the session is deleted, **then** the `CategoryDeletedEvent`
  handler removes it from the session's selected categories.
- **Given** another user's session/sub-resources, **then** access is treated as not found.
- **Given** either API (5000/5001), **then** the routes and behavior are identical.

## Data touched
`randomizations`, `selectedCategories`, `usedQuestions`, `postponedQuestions`. Shapes:
[`schema.json`](../schema.json). Full endpoint list: [`api.md`](../api.md).

## Out of scope
The random-selection algorithm (client-side); question CRUD (see [question-management](question-management.md)).
