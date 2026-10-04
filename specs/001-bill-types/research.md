# Research: Bill Types

**Date**: 2026-06-13 | **Plan**: [plan.md](plan.md)

## Decision 1: Where does billing type live in the data store?

**Decision**: Use the existing billing type attribute on the NocoDB `bills` table. No new column or schema change is needed.

**Rationale**: The attribute already exists in the data store and is already fetched and returned by the `bills` service. The only gap is that bill creation does not yet accept it as an input.

**Alternatives considered**: Adding a separate dedicated field with a different name — rejected because the existing attribute already serves this purpose and renaming would require a data store schema change with no benefit.

---

## Decision 2: How is billing type accepted on bill creation?

**Decision**: Extend the bill creation input in the `bills` service to accept billing type as an optional field, defaulting to `monthly` when omitted.

**Rationale**: This is the minimal change needed — the service already returns billing type on reads. Making it writable on create closes the only gap without touching any other operation.

**Alternatives considered**: Hardcoding the default in the service and never exposing it as an input — rejected because users need to specify yearly and prepaid types at creation time.

---

## Decision 3: How is immutability enforced?

**Decision**: Immutability is enforced at the UI layer (no edit control shown) and by omission at the API layer (no update-billing-type operation exists).

**Rationale**: There is no existing update operation that could be misused. Adding a server-side guard would be over-engineering for a single-user homelab (YAGNI, Constitution Principle V).

**Alternatives considered**: Server-side guard rejecting billing type on any update operation — deferred; can be added if an update operation is introduced in the future.

---

## Decision 4: How are legacy records (no billing type) handled?

**Decision**: Apply a `monthly` default at display time in the UI. No data migration.

**Rationale**: Records created before this feature have no billing type stored. A display-time fallback is sufficient at homelab scale and avoids any migration risk.

**Alternatives considered**: Setting a default value at the data store level — possible but requires an admin action outside the codebase; display-time fallback is simpler and reversible.

---

## Decision 5: How is billing type selection presented in the UI?

**Decision**: Inline toggle/pill buttons on the add bill form (monthly | yearly | prepaid), with monthly pre-selected. On the bill list and detail views, billing type is shown as a read-only badge/pill with no edit control.

**Rationale**: A small fixed set of mutually exclusive options is best represented as visible toggles rather than a collapsed dropdown, keeping all choices visible at a glance. The read-only badge is visually consistent with the creation control and makes the immutability clear without hiding information.

**Alternatives considered**: Dropdown select — rejected in favour of pill buttons for discoverability given only three options.
