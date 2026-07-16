# Feature: Admin

## Purpose
Expose administrative/introspection endpoints available only to admin users.

## Actors
Authenticated user holding the admin role. Guarded by `AdminPolicy`.

## Behavior
- `GET /api/admin/me` — returns the authenticated admin's identity/claims.
- `GET /api/admin/health` — returns a system-health view for admins.
- `GET /api/admin/users` — lists users.
- `GET /api/admin/analytics` — returns system analytics.

## Acceptance criteria
- **Given** a non-admin (or unauthenticated) caller, **when** they hit any `/api/admin/*` endpoint,
  **then** the request is rejected by `AdminPolicy` (403/401).
- **Given** an admin caller, **when** they call `/api/admin/me`, **then** their identity/claims are
  returned.
- **Given** an admin caller, **when** they call `/api/admin/health`, **then** a system-health view is
  returned.
- **Given** an admin caller, **when** they call `/api/admin/users` or `/api/admin/analytics`,
  **then** the user list / system analytics are returned.
- **Given** either API, **then** behavior is identical.

## Data touched
Identity/claims from the Firebase token; role from the `users` collection. Authorization model:
[`authentication-and-authorization.md`](authentication-and-authorization.md).

## Out of scope
The unauthenticated liveness endpoint `GET /health` (see [`api.md`](../api.md)).
