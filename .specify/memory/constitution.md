<!--
SYNC IMPACT REPORT
==================
Version change: [TEMPLATE] → 1.0.0
Modified principles: N/A (initial population from template)
Added sections: Core Principles (I–V), Deployment Model, Development Workflow, Governance
Removed sections: None (template placeholders replaced)
Templates requiring updates:
  - .specify/templates/plan-template.md ✅ (constitution check gates align with principles)
  - .specify/templates/spec-template.md ✅ (no mandatory section changes required)
  - .specify/templates/tasks-template.md ✅ (no new principle-driven task types)
Follow-up TODOs:
  - TODO(RATIFICATION_DATE): Exact original project adoption date unknown; marked as project start estimate.
-->

# Payobills Constitution

## Core Principles

### I. Monorepo with App Isolation

Each application MUST live in its own directory under `apps/`. Shared utilities and libraries MUST go in `common/`. No app may directly import another app's internal code — cross-app dependencies MUST flow through `common/`. This ensures each service can be reasoned about, deployed, and changed independently.

### II. Helm-Based Deployment

Every application MUST be packaged and deployed as a Helm chart. There MUST be no ad-hoc `kubectl apply` workflows for application-level resources. Chart values MUST be the single source of truth for environment-specific configuration. This aligns with the homelab Kubernetes cluster managed by the sibling project `kube-homelab`.

### III. Homelab-First, kube-homelab Dependency

The deployment target is the local Kubernetes cluster provided by `kube-homelab` (sibling project). Deployments MUST assume `kube-homelab` is operational. Infrastructure concerns (ingress, storage classes, cluster DNS) are the responsibility of `kube-homelab`, not this project. This project MUST NOT duplicate cluster-level infrastructure definitions.

### IV. Test Coverage is Aspirational, Not Blocking

The project currently lacks tests in many areas. Tests SHOULD be added incrementally. Absence of tests MUST NOT block feature development or deployment. When tests are added, they MUST be meaningful and cover user-facing behavior. Test-driving new features is encouraged but not mandated at this stage.

### V. Simplicity and Self-Hosted Data Ownership

Features MUST favor simplicity over cleverness. The core value proposition is self-hosted, user-owned financial data — no feature may compromise data locality or require external cloud services to function. YAGNI applies: implement what is needed now, not hypothetical future needs.

## Deployment Model

All apps are deployed to a Kubernetes cluster managed by `kube-homelab`. Each app has its own Helm chart. The `common/` directory contains shared Docker base images, k8s helpers, and reusable chart templates. Deployment pipeline changes MUST be validated against the running homelab cluster before merging.

Apps:
- `bills` — Core bill management service
- `bills-parser` — Parses bill documents
- `files` — File storage service
- `ocr` — OCR processing
- `payments` — Payment recording
- `transaction-parser` — SLM-powered transaction parsing (runs on local PC, not RPi)
- `transaction-syncer` — Syncs transactions from external sources
- `ui` — Frontend application
- `user` — User management service

## Development Workflow

- Branches MUST be rebased against `origin/main` (never `develop` or `master` for rebase).
- PRs target the `develop` branch for integration, `main` for releases.
- Each app is independently deployable; deploy only changed apps when possible.
- CSS MUST NOT use negative margins or negative padding — restructure layout instead.
- `.tool-versions` MUST NOT be modified; it pins the Rust toolchain for CI.

## Governance

This constitution supersedes all informal project conventions. Amendments require:
1. A pull request updating this file with rationale.
2. Version increment per semantic versioning rules (MAJOR: principle removal/redefinition; MINOR: new principle/section; PATCH: clarification/wording).
3. Sync impact report updated in the HTML comment above.

All feature plans MUST include a Constitution Check gate verifying compliance with principles I–V before implementation begins.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): use project start date ~2023 | **Last Amended**: 2026-06-11
