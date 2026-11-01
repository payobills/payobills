# Implementation Plan: Multiple Users and Shared Bills

**Branch**: `feat-multi-user-bills` | **Date**: 2026-10-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-multi-user-bills/spec.md`

## Summary

Delivered as four slices: auth, users, sharing, then receipts without a bill cycle. Auth makes every user-facing service verify Keycloak tokens, behind `AUTH__ENFORCE`. Users adds per-person scoping, sharing adds a membership table, and receipts adds a cycle-free upload on the bill page. Design: [hld.md](hld.md), [lld.md](lld.md), [service-contracts.md](service-contracts.md), decisions in [research.md](research.md).

## Technical Context

**Language/Version**: .NET 8 with HotChocolate 14.3 (bills, payments), Node with Express 5 and Apollo Server (files, user), Rust (transaction-syncer), SvelteKit (ui)

**Primary Dependencies**: Keycloak (existing), NocoDB (existing), JWT validation for .NET and Node

**Storage**: NocoDB `payobills` base

**Testing**: xUnit in `apps/bills/tests`. New cross-user access test script for SC-001

**Target Platform**: kube-homelab Kubernetes on Raspberry Pi nodes

**Project Type**: Federated GraphQL microservices with a web UI

**Performance Goals**: No perceptible slowdown with 50 bills and 10,000 transactions (SC-005)

**Constraints**: No external cloud dependency, first user never locked out during rollout

**Scale/Scope**: A household-sized number of users, 4 services, UI, syncer, 2 new tables, 6 changed tables

## Constitution Check

| Principle | Status | Note |
|---|---|---|
| I. App isolation | Deviation | Auth helper is per service for now, see Complexity Tracking |
| II. Helm deployment | Pass | New settings go through chart values |
| III. kube-homelab | Pass | No new infrastructure |
| IV. Tests aspirational | Pass | Access tests included because this is a security boundary |
| V. Simplicity and self-hosted | Pass | Uses existing Keycloak and NocoDB, no cloud service |
| VI. HLD discipline | Pass | `hld.md` has no code or class names |
| VII. Artifact structure | Pass | Exactly the seven allowed artifacts, so no `tasks.md` |
| VIII. Diagrams | **Blocked** | Diagrams are Mermaid, but Docker is not running so validation has not been done |

Auth implementation started at the project owner's direction with Principle VIII still open. These documents must not be marked ready until the diagrams pass validation.

## Project Structure

```text
specs/002-multi-user-bills/   Design artifacts
apps/bills/src/               Token checks, scoping, membership mutations
apps/payments/src/            Scoping, filter escaping
apps/files/src/               Identity, file access rules
apps/user/src/                Real accounts in place of the mock
apps/transaction-syncer/src/  Import owner
apps/ui/src/src/              Tokens, file loading, sharing screens, cache rules
apps/*/k8s/values*.yaml       New settings
```

**Structure Decision**: Existing monorepo layout, with no new directories outside `specs/`.

## Tasks

Four slices, delivered in this order. Each slice is one or more PRs, is finished before the next starts, and leaves the app working.

### Slice 1: Auth (only signed-in people can reach the app and APIs)

- [ ] T001 Confirm the gateway forwards `Authorization`, and update its config if not (outside this repo)
- [x] T002 Add an auth helper to each service (research R10)
- [x] T003 UI: keep the access token in memory, refresh it, and attach it to GraphQL requests
- [x] T004 UI: add `authFetch` and use it for all `/files` calls and file display
- [x] T005 UI: restore the login guard in `routes/+layout.svelte`
- [x] T006 UI: keep private data out of the service worker cache, and clear caches on logout
- [x] T007 Bills, payments, files and user: verify tokens, with `AUTH__ENFORCE=false`
- [x] T008 Add chart values for the auth settings
- [ ] T009 Set `AUTH__ENFORCE=true` and confirm requests without a token get 401 on every route

### Slice 2: Users (each person sees only their own data)

- [ ] T010 Escape filter input and validate IDs in payments and bills (security fix)
- [ ] T011 Add the tables and columns from `lld.md` to NocoDB, scripted alongside the existing NocoDB import scripts
- [ ] T012 Verify the `~or` access filter on the owner and bill-link columns against the running NocoDB
- [ ] T013 Write a re-runnable backfill and test it on a data copy, comparing counts and totals (SC-002)
- [ ] T014 User service: real accounts, `me` creates on first call
- [ ] T015 Syncer: set `IMPORT_OWNER_SUBJECT` on imported transactions
- [ ] T016 Run the backfill in production
- [ ] T017 Bills: identity and owner scoping on all operations, including `billStats`
- [ ] T018 Payments: scoping for transactions, stats, tags and trips
- [ ] T019 Files: owner on upload, read-time access rules, tag target checks
- [ ] T020 Re-run the backfill for rows created meanwhile
- [ ] T021 Write and run the cross-user access script covering every operation in `service-contracts.md` (SC-001)

### Slice 3: Sharing

- [ ] T022 Bills: `billMemberAdd`, `billMemberRemove`, `billLeave`, `members`, `myRole`, `createdBy`
- [ ] T023 Payments and files: grant access through bill membership, and check membership when linking a transaction to a bill
- [ ] T024 UI: member management component and shared markers
- [ ] T025 Extend the access script with member, removed-member and non-member cases

### Slice 4: Receipts without a bill cycle

- [ ] T026 Files: accept and authorize a receipt linked directly to a bill through `BillId`
- [ ] T027 Bills: list a bill's receipts
- [ ] T028 UI: add receipt action and list on the bill page, with no bill cycle field
- [ ] T029 Extend the access script with the new receipt routes

## Risks

| Risk | Mitigation |
|---|---|
| Access gap in one of many operations | T021 tests every listed operation, and identity comes from one helper |
| Being locked out at enforcement | Flag can be reverted without a code change. Confirm the UI sends tokens through the gateway before T009 |
| Wrong issuer, or pods unable to reach the IdP, refuses every token even with enforcement off | Deploy services and confirm one signed-in request works, and revert the UI if not |
| Compose hook breaks under enforcement if rover uses `subgraph_url` | Check the rover config before T009 |
| Any Keycloak realm user can see all data between T009 and T019 | Check who is in the realm before T009, and keep the gap short |
| First user sees an empty app if scoping ships before backfill | T016 runs before T017, and T020 catches later rows |
| Local `apps/mcp` and `apps/agent` call the gateway without tokens | They break at T009 and need a service account or token. They are untracked locally, so they are not covered here |

## Complexity Tracking

| Deviation | Why | Simpler or compliant alternative rejected because |
|---|---|---|
| Principle I: auth helper copied per service instead of living in `common/` | Docker contexts contain only each app's `src/`, so sharing changes all four image builds | Those builds cannot be tested without Docker, and a broken pipeline costs more than two small copies. Move to `common/` once they can be tested |
