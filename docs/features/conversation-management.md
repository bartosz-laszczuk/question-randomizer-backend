# Feature: Conversation management

## Purpose
Persist and manage AI-agent conversations and their messages, so the agent can retain multi-turn
context and the frontend can display chat history.

## Actors
Authenticated user (owner). Conversations module.

## Behavior
- Create, read, and **hard-delete** conversations. There is **no general update** endpoint — only
  an **update-timestamp** action that touches `updatedAt`.
- List and append messages for a conversation; a message has a `role` (`user` | `assistant`) and
  `content`.
- Appending a message (and update-timestamp) refreshes `updatedAt`; conversations list newest-first
  by `updatedAt`.
- All operations verify ownership by `userId`.

## Acceptance criteria
- **Given** an authenticated user, **when** they create a conversation, **then** it is persisted to
  `conversations` with their `userId`.
- **Given** a conversation they own, **when** they add a message, **then** it is persisted to
  `messages` with the `conversationId` and the conversation's `updatedAt` is refreshed.
- **Given** a conversation owned by another user, **when** the caller reads/updates/deletes/appends,
  **then** it is treated as not found.
- **Given** a delete, **then** the conversation document is hard-deleted.
- **Given** either API, **then** behavior is identical.

## Data touched
`conversations`, `messages`. Shapes: [`schema.json`](../schema.json).

## Dependencies
Consumed by the agent for conversation-context memory (see [ai-agent](ai-agent.md)).

## Out of scope
Agent execution/streaming (see [ai-agent](ai-agent.md)).
