# Feature: Authentication & authorization

## Purpose
Ensure every request is authenticated as a known Firebase user and that users can only act on their
own data, with elevated endpoints restricted to admins.

## Actors
Every caller. Enforced in the SharedKernel (auth middleware, `ICurrentUserService`, policies).

## Behavior
- Each request (except `/health`) must carry `Authorization: Bearer <Firebase ID token>`; the token
  is verified and the `userId` extracted via `ICurrentUserService`.
- Handlers/repositories scope all operations by that `userId` and verify ownership before
  read/update/delete — never trusting a client-supplied id.
- Policy-based authorization guards elevated endpoints (`AdminPolicy` on `/api/admin/*`).

## Acceptance criteria
- **Given** no/invalid token, **when** any protected endpoint is called, **then** it returns 401.
- **Given** a valid token, **then** the resolved `userId` is the only identity used server-side;
  a client-provided `userId` is ignored.
- **Given** a resource owned by another user, **when** accessed, **then** it is treated as not
  found (no cross-user reads or writes).
- **Given** a non-admin, **when** calling `/api/admin/*`, **then** `AdminPolicy` denies it.
- **Given** either API, **then** enforcement is identical.

## Data touched
Firebase Auth (identity); `users` (roles). Full model: [`guides/AUTHORIZATION.md`](../guides/AUTHORIZATION.md).

## Out of scope
Per-feature CRUD behavior (see each feature spec).
