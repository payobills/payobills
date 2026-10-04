# High-Level Design: Bill Types

**Date**: 2026-06-13 | **Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

## Systems Involved

| System | Role | Change Required |
|--------|------|----------------|
| NocoDB (`bills` table) | Data store for bill records | None — billing type attribute already present |
| `bills` service (GraphQL API) | Exposes bill data; accepts bill creation requests | Bill creation contract must accept billing type as optional input; existing bill retrieval already returns it |
| `ui` (web application) | User-facing interface | Add bill form needs a billing type selector; bill list and detail views need a read-only billing type indicator |

**No new services or infrastructure components are introduced.**

## Data Flow: Create Bill with Billing Type

```mermaid
sequenceDiagram
    actor User
    participant UI as ui (SvelteKit)
    participant Bills as bills service (GraphQL)
    participant NocoDB

    User->>UI: Selects billing type and submits add bill form
    UI->>Bills: addBill mutation (name, billingPeriod, ...)
    Bills->>NocoDB: Create bill record with BillingPeriod
    NocoDB-->>Bills: Created bill record
    Bills-->>UI: BillDTO (id, name, billingPeriod, ...)
    UI-->>User: Bill added and billing type badge shown in list
```

## Data Flow: View Bill

```mermaid
sequenceDiagram
    actor User
    participant UI as ui (SvelteKit)
    participant Bills as bills service (GraphQL)
    participant NocoDB

    User->>UI: Navigates to bills list or bill detail
    UI->>Bills: bills query or billById query
    Bills->>NocoDB: Fetch bill records (BillingPeriod included)
    NocoDB-->>Bills: Bill records
    Bills-->>UI: BillDTO list (billingPeriod populated)
    UI-->>User: Billing type shown as read-only badge/pill
```

## Billing Type Lifecycle

```mermaid
flowchart LR
    A([Bill Created]) --> B{BillingPeriod set}
    B -->|monthly| C[monthly - immutable]
    B -->|yearly| D[yearly - immutable]
    B -->|prepaid| E[prepaid - immutable]
    C --> F([No further transitions])
    D --> F
    E --> F
```

## Immutability Constraint

Billing type is set at creation and never changed. This is enforced at the UI layer (no edit control shown) and by omission at the API layer (no update-billing-type operation exists).
