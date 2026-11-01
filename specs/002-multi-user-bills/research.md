# Research: Multiple Users and Shared Bills

## Current state (origin/main at cd11659)

| Area | Finding | Evidence |
|---|---|---|
| Sign-in | Keycloak login exists in the UI only. The access token is discarded and only the refresh token is kept | `apps/ui/src/src/routes/callback/+page.svelte` |
| Login guard | The shared guard is commented out. Only some pages, such as `/timeline` and `/bills/add`, redirect on their own, while `/transactions` and `/trips` do not | `apps/ui/src/src/routes/+layout.svelte:32` |
| Bill cycle | Statement upload requires a bill period, and the statement page ties files to a cycle. Transaction receipts have no cycle, but need an existing transaction | `lib/bill-upload-statement.svelte`, `routes/transaction/+page.svelte` |
| APIs | No token is sent or checked anywhere | `apps/ui/src/src/lib/stores/urql.ts`, no auth code in bills, payments, files or user |
| User service | Returns a hardcoded user | `apps/user/src/index.mjs` |
| Ownership | No owner on any record | `apps/bills/src/Data.Contracts/Models/Bill.cs` |
| Internal services | Parsers and syncer write to NocoDB directly, not through these APIs | `apps/transaction-syncer/src/main.rs:80` |
| Gateway | Its configuration is not in this repo | Only the supergraph compose hook is present |

## Decisions

### R1. Validate tokens in each service

- **Decision**: bills, payments, files and user each validate the token. The gateway only forwards it.
- **Rationale**: the local ingress manifest routes `/bills-graphql`, `/payments-graphql` and `/files` directly to services, so gateway-only checks could be bypassed. Apollo's edge JWT runs on a GraphOS plan, which conflicts with Principle V (no external cloud service required).
- **Alternatives**: ingress-level auth proxy (more moving parts, and services would still trust a header); gateway-only validation (bypassable).

### R2. Use the Keycloak subject as the user ID

- **Decision**: owner and member columns store the token's `sub` claim.
- **Rationale**: every service gets identity from the token with no lookup. Client-supplied identity is never used (FR-005).
- **Alternatives**: internal numeric IDs (a user lookup on every request).

### R3. Store bill ownership as a membership row

- **Decision**: one `bill_members` table with roles `owner` and `member`, and no owner column on bills.
- **Rationale**: one source of truth for access, and "my bills" is a single query.
- **Alternatives**: owner column plus member table (two sources that can disagree).
- **Risk**: NocoDB has no composite unique constraint, so duplicates are prevented by checking before insert. A rare race only creates a harmless duplicate row, and removal deletes all matching rows.

### R4. Enforce access with server-side filters

- **Decision**: a transaction is accessible if the caller owns it or its bill link (`nc_14ri__bills_id`) is in their bills. Statements and payments check the same link.
- **Rationale**: filtering after a page is fetched would give wrong totals and short pages.
- **Verify first**: `eq` on this link column is already used (`BillStatementsNocoDBService.cs:31`). Combining `~or` across that column and an owner column is unverified against the running NocoDB version (plan task T012).

### R5. Close filter injection before relying on filters

- **Decision**: IDs must be numeric. `SearchTerm` and `OcrId` must be escaped, and the access clause wraps the whole client filter in an outer `~and`.
- **Rationale**: `TransactionsNocoDBService.cs` and `GetBillsByIdsAsync` put client values into `w=` unescaped, so a crafted value could add its own `~or` clause and bypass ownership. FR-004 is not met without this.

### R6. Resolve file access at read time through existing links

- **Decision**: a file is accessible to its uploader, or to anyone who can access a transaction, statement or bill linked to it.
- **Rationale**: following links means a transaction moved to another bill needs no file updates.
- **Alternatives**: copying a bill ID onto each file (goes stale on moves).
- **Related**: `TransactionReceiptsSync` attaches files by a client-set tag, so upload must check that the caller can access the target. Otherwise one user could attach files to another's transaction.

### R7. Load files with a token, not a plain URL

- **Decision**: the UI fetches files with the token and displays them through an object URL.
- **Rationale**: `<img src>` and download links cannot send an Authorization header (`lib/file-uploader.svelte:65`).
- **Alternatives**: short-lived signed links (new signing mechanism, and links can leak).

### R8. Stop caching private responses on the device

- **Decision**: the service worker never handles `/files` or `/gateway`, and sign-out clears its `offline*` caches. The lite-mode IndexedDB is kept.
- **Rationale**: `fetchAndCache` currently caches every GET and serves it when offline, so a file could reach the next person on a shared device (FR-014). Lite mode's IndexedDB may be the only copy of that data, so clearing it on sign-out would lose it.

### R9. Roll out behind a flag

- **Decision**: `AUTH__ENFORCE=false` lets requests without a token through, and a present but invalid token is always rejected. Turning the flag on finishes the auth slice, before any ownership code ships.
- **Rationale**: FR-015 needs services and UI to deploy independently without locking out the first user. Because enforcement comes first, scoped code always has an identity, so no "missing token means first user" fallback is needed.
- **Risk**: APIs stay as open as they are today until enforcement is on, and between enforcement and scoping any user in the Keycloak realm can see all data. Both windows should be short.

### R10. Keep the auth helper in each service for now

- **Decision**: one file per service: `API/Auth.cs` in bills and payments, and `src/auth.js` and `src/auth.mjs` in files and user. This deviates from Principle I, see Complexity Tracking in `plan.md`.
- **Rationale**: Docker build contexts contain only each app's `src/`, and no app consumes `common/` code today. Sharing means changing all four image builds, which could not be tested because Docker is unavailable. Bills and payments already each carry their own NocoDB client.
- **Alternatives**: `common/auth/` (Principle I compliant, move there once image builds can be tested).

## To confirm during implementation

- The gateway forwards the Authorization header. Its config lives outside this repo.
- The supergraph compose hook still works under enforcement. `roverConfig` is empty in this repo, and a `subgraph_url` schema source POSTs without a token, which enforcement refuses. Only `GET /graphql?sdl` (and `GET /sdl` on user) is public.
- Pods can reach the issuer URL to fetch signing keys. If they cannot, every token is refused, even with enforcement off.
- Keycloak access tokens carry `email` once the scope is requested, and `azp` matches the UI client. Audience is checked through `azp` because Keycloak's default audience is `account`.
- Each environment's token issuer. Local and production use different IdP hosts (`apps/ui/src/src/utils/auth.ts`).

## Sources

- [Apollo Router JWT authentication](https://www.apollographql.com/docs/deploy-preview/506da8e589f9b712b544cc15/graphos/routing/security/jwt)
- [Apollo Router authentication overview](https://www.apollographql.com/docs/deploy-preview/54d13caf310343750bbf6356/graphos/routing/security/router-authentication)
- [Auth in a supergraph tutorial](https://www.apollographql.com/tutorials/voyage-part2/04-auth-in-a-supergraph)
