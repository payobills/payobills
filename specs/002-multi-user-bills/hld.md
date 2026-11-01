# High-Level Design: Multiple Users and Shared Bills

## Overview

People sign in through the existing identity provider. The UI sends proof of identity with every request, and each user-facing service verifies it independently and limits data to what that person may access. Sharing is a membership relationship between a person and a bill, stored alongside existing data.

## Systems

| System | Role in this feature |
|---|---|
| UI | Keeps a session, attaches identity to every request, adds sharing screens, clears device data on sign-out |
| Identity provider | Authenticates people and issues signed identity tokens. Unchanged except for requesting email |
| Gateway | Forwards identity to the services behind it |
| User service | Creates accounts on first sign-in and supplies names |
| Bills service | Limits bills, statements and payments to members, and manages membership |
| Payments service | Limits transactions, tags and trips to their owner or to bill members |
| Files service | Limits files to their uploader or to people who can access the linked record |
| Data store | Holds accounts, memberships and new owner fields |
| Syncer | Marks imported transactions with the configured import owner |
| Parsers and OCR | Unchanged. Internal only, not reachable from outside the cluster |

## Request flow

```mermaid
sequenceDiagram
    participant Person
    participant UI
    participant IdP as Identity provider
    participant Gateway
    participant Service as Bills, payments or files
    participant Store as Data store
    Person->>UI: Sign in
    UI->>IdP: Authenticate
    IdP-->>UI: Signed identity token
    UI->>Gateway: Request with token
    Gateway->>Service: Forward request and token
    Service->>Service: Verify token signature, issuer, expiry
    Service->>Store: Read data limited to this person
    Store-->>Service: Permitted records only
    Service-->>Gateway: Response
    Gateway-->>UI: Response
```

Requests that reach a service directly follow the same verification, so bypassing the gateway grants nothing.

## Access rules

```mermaid
flowchart TD
    A[Request for a record] --> B{Valid identity?}
    B -- No --> R[Refuse]
    B -- Yes --> C{Owned by this person?}
    C -- Yes --> G[Allow]
    C -- No --> D{Linked to a bill they belong to?}
    D -- Yes --> G
    D -- No --> R
```

Refusals look the same as a record that does not exist, so they reveal nothing about other people's data.

## Sharing flow

```mermaid
sequenceDiagram
    participant Owner
    participant UI
    participant Bills as Bills service
    participant Store as Data store
    participant Member
    Owner->>UI: Add a person by email
    UI->>Bills: Add member to bill
    Bills->>Store: Find account for email
    Store-->>Bills: Account found or not found
    Bills-->>UI: Added, or person must sign in first
    Member->>UI: Open bills
    UI->>Bills: List bills
    Bills-->>UI: Own and shared bills
```

## Rollout

Each step leaves the first user working:

- Auth: release the UI so that it sends identity, release services that verify identity when present, then refuse requests without identity.
- Users: add new data fields, assign all existing records to the first user, then limit data to each person.
- Sharing: add memberships and sharing screens.
- Receipts: add receipt upload on the bill page, with no bill cycle.

## Out of scope

Cost splitting, ownership transfer, per-person imports, and a registration screen.
