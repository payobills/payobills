# Implementation Plan: Bill Types

**Branch**: `feat/use-sdd` | **Date**: 2026-06-11 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-bill-types/spec.md`

## Summary

Extend the bill creation flow to support three billing types: monthly (default), yearly, and prepaid. The data store already holds the billing type attribute; the gaps are in the API contract (bill creation does not yet accept a billing type) and the UI (no billing type selector on the add-bill form, no billing type display in the bills list or detail view). Billing type is immutable once a bill is created.

## Technical Context

**Backend service**: `bills` — GraphQL API, backed by NocoDB as the data store

**Frontend**: `ui` — SvelteKit web application, communicates with `bills` via GraphQL

**Data store**: NocoDB (`bills` table). The billing type attribute already exists in the store. No schema changes required.

**Testing**: Unit tests exist for the `bills` service; no automated UI tests (aspirational per constitution)

**Target Platform**: Kubernetes homelab cluster

**Performance Goals**: No new targets — additive feature, same homelab scale

**Constraints**: No negative CSS margins/padding; no new services; data locality preserved; YAGNI

**Scale/Scope**: Single homelab user; small bill count (<100 records)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Monorepo / App Isolation | ✅ Pass | Changes confined to `apps/bills` and `apps/ui`; no cross-app direct imports |
| II. Helm-Based Deployment | ✅ Pass | No new services; existing Helm charts unchanged |
| III. Homelab-First | ✅ Pass | No external cloud dependencies introduced |
| IV. Tests Aspirational | ✅ Pass | Existing unit tests updated where applicable; absence not blocking |
| V. Simplicity / Data Ownership | ✅ Pass | Additive field on existing entity; no external services; YAGNI applied |
| VI. HLD Design Discipline | ✅ Pass | Plan describes services and interactions only; implementation details are in lld.md and service-contracts.md |
| VII. Feature Artifact Structure | ✅ Pass | Directory contains exactly the canonical artifacts: spec.md, hld.md, lld.md, service-contracts.md, research.md, plan.md, checklists/requirements.md |
| VIII. Diagram Standards | ✅ Pass | lld.md Data Model Changes section has no diagram (no data model changes per v1.3.3); HLD diagrams are Mermaid |

**All gates pass.**

## Project Structure

### Documentation (this feature)

```text
specs/001-bill-types/
├── checklists/
│   └── requirements.md  ← spec quality checklist
├── research.md          ← research findings
├── spec.md              ← feature specification
├── hld.md               ← high-level design (systems and interactions)
├── lld.md               ← low-level design + data model
├── service-contracts.md ← GraphQL API contracts
└── plan.md              ← this file
```

See [hld.md](hld.md) for the systems involved and data flows.

## Validation

End-to-end validation scenarios (run after deployment):

1. **Monthly default** — Create a bill without selecting a billing type; verify it appears in the list as "monthly".
2. **Yearly** — Create a bill selecting "yearly"; verify the badge shows "yearly" in list and detail views.
3. **Prepaid** — Create a bill selecting "prepaid" without filling billing/pay-by dates; verify it saves successfully and shows "prepaid".
4. **Immutability** — Open an existing bill; verify billing type is shown as a non-interactive badge with no edit control.
5. **Legacy fallback** — Find a NocoDB record with no `BillingPeriod` value; verify it displays as "monthly" with no error.

## Complexity Tracking

*No constitution violations — table not required.*
