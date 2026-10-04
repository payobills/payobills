# Feature Specification: Bill Types

**Feature Branch**: `feat/bill-types`

**Created**: 2026-06-11

**Status**: Draft

**Input**: User description: "Bills in payobills can now be of multiple types - monthly (default), yearly or prepaid"

## Clarifications

### Session 2026-06-11

- Q: Can users change the billing type of an existing bill? → A: No. Billing type is immutable once a bill is created.
- Q: What is the NocoDB entity shape for Bill? → A: Existing `Bill` model has fields: `Id` (long), `Name` (string), `BillingDate` (int?), `PayByDate` (int?), `LatePayByDate` (int?), `IsEnabled` (bool), `PrimaryType` (string?), `BillingPeriod` (string), `Payments` (list), `CreatedAt`, `UpdatedAt`. The `BillingPeriod` field is the target attribute for billing type.
- Q: How should billing type be selected in the "add bill" form? → A: Inline toggle/pill buttons showing all three options (monthly, yearly, prepaid), with "monthly" pre-selected.
- Q: How should billing type appear when viewing or editing an existing bill? → A: Read-only badge/pill — visually consistent with the creation toggle, but non-interactive (greyed out or in a locked selected state).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create a Monthly Bill (Priority: P1)

A user creates a new bill and it defaults to the "monthly" billing cycle. This is the most common case and ensures no disruption to existing workflows.

**Why this priority**: Monthly bills are the default and most common type. Existing bill creation must continue to work seamlessly.

**Independent Test**: Can be tested by creating a bill without specifying a type and verifying it is recorded as "monthly".

**Acceptance Scenarios**:

1. **Given** a user is creating a new bill, **When** they do not specify a billing type, **Then** the bill is created with `BillingPeriod` = "monthly" by default.
2. **Given** an existing monthly bill, **When** the user views it, **Then** the billing type is displayed as "monthly".

---

### User Story 2 - Create a Yearly Bill (Priority: P2)

A user creates a bill for a service that charges annually (e.g., insurance, software subscriptions), selecting "yearly" as the billing type.

**Why this priority**: Yearly bills represent a significant real-world billing pattern and must be trackable separately from monthly cycles.

**Independent Test**: Can be tested by creating a bill with type "yearly" and verifying it is stored and displayed correctly.

**Acceptance Scenarios**:

1. **Given** a user is creating a new bill, **When** they select "yearly" as the billing type, **Then** the bill is created with `BillingPeriod` = "yearly".
2. **Given** a yearly bill, **When** the user views the bill list, **Then** the bill is labeled or identified as "yearly".

---

### User Story 3 - Create a Prepaid Bill (Priority: P2)

A user creates a bill for a prepaid service (e.g., prepaid mobile, prepaid electricity units), selecting "prepaid" as the billing type.

**Why this priority**: Prepaid services have no fixed recurrence; the system must support tracking them without assuming a billing cycle.

**Independent Test**: Can be tested by creating a bill with type "prepaid" and verifying it is stored without requiring a billing cycle date.

**Acceptance Scenarios**:

1. **Given** a user is creating a new bill, **When** they select "prepaid" as the billing type, **Then** the bill is created with `BillingPeriod` = "prepaid".
2. **Given** a prepaid bill, **When** the user views the bill, **Then** it is clearly identified as prepaid with no implied billing cycle.

---

### UX Interaction Notes

- The "add bill" form presents billing type as a row of three inline toggle/pill buttons: **monthly** | **yearly** | **prepaid**. "Monthly" is pre-selected when the form opens.
- Only one option can be active at a time (mutually exclusive selection).
- When "prepaid" is selected, the `BillingDate` and `PayByDate` fields are hidden or marked optional, as they are not required for prepaid bills.
- On the bill list, the billing type is shown as a read-only badge/pill alongside the bill name.
- On the bill detail/edit view, billing type is shown as a read-only badge/pill that is visually consistent with the creation toggle buttons but non-interactive (greyed out or locked selected state). No edit control is presented.
- If a bill has no stored billing type (legacy data), it displays with no badge or as "monthly" — consistent with the backwards-compatibility default.

### Edge Cases

- What happens when an existing bill record has no `BillingPeriod` set (legacy data)? It should be treated as "monthly" to maintain backwards compatibility.
- How does the system handle a billing type that is not one of the three allowed values? It should reject the value with a clear error.
- What happens to payment due dates (`PayByDate`, `LatePayByDate`) for a "prepaid" bill? Due dates are not required/enforced for prepaid bills.
- Can the billing type be changed after a bill is created? No — `BillingPeriod` is set at creation and is immutable thereafter.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support three bill types stored in the `BillingPeriod` field: "monthly", "yearly", and "prepaid".
- **FR-002**: When creating a bill, if no billing type is specified, the system MUST default `BillingPeriod` to "monthly".
- **FR-003**: Users MUST be able to select the billing type when creating a new bill.
- **FR-004**: Once a bill is created, its billing type (`BillingPeriod`) MUST NOT be changeable. Any attempt to update the billing type of an existing bill MUST be rejected.
- **FR-005**: System MUST display the billing type for each bill in the bill list and bill detail views.
- **FR-006**: System MUST reject any billing type value not in the set {monthly, yearly, prepaid} with a user-visible error message.
- **FR-007**: Existing bill records with no `BillingPeriod` value MUST be treated as "monthly" (backwards compatibility, no data migration required).
- **FR-008**: Prepaid bills MUST NOT require `BillingDate`, `PayByDate`, or `LatePayByDate` to be set.
- **FR-009**: The "add bill" form MUST present billing type as a row of inline toggle/pill buttons (monthly | yearly | prepaid) with "monthly" pre-selected.
- **FR-010**: On bill detail and edit views, billing type MUST be displayed as a non-interactive read-only badge/pill; no edit control for billing type is shown.

### Key Entities

- **Bill**: Represents a recurring or prepaid financial obligation. Sourced from NocoDB. Relevant fields:
  - `Id` (long) — unique identifier
  - `Name` (string) — bill display name
  - `BillingPeriod` (string) — billing type; allowed values: `monthly`, `yearly`, `prepaid`; defaults to `monthly`; immutable after creation
  - `BillingDate` (int?) — day of month for billing; not required for prepaid bills
  - `PayByDate` (int?) — due date; not required for prepaid bills
  - `LatePayByDate` (int?) — late payment cutoff; not required for prepaid bills
  - `IsEnabled` (bool) — whether the bill is active
  - `PrimaryType` (string?) — existing categorization field (unaffected by this feature)
  - `CreatedAt` / `UpdatedAt` — audit timestamps

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All three billing types can be created and viewed without errors.
- **SC-002**: 100% of bills created without an explicit type are recorded and displayed as "monthly".
- **SC-003**: Existing bills with no billing type stored display and function as "monthly" with no data migration failures.
- **SC-004**: Users can distinguish billing types in the bill list at a glance (type is visible without opening the bill detail).
- **SC-005**: Invalid billing type values and attempts to change billing type on existing bills are rejected with a user-visible error.

## Assumptions

- Users interact with bills through the existing `ui` app and `bills` service; no new app is introduced.
- `BillingPeriod` is the NocoDB field used to store billing type; no schema changes to other fields are required.
- Billing type is a property of the bill record itself, not of individual payment records.
- The "prepaid" type does not imply a fixed amount; it only describes the billing cycle model.
- No external integrations or notifications are affected by billing type in this phase (YAGNI).
- Data backwards compatibility for legacy records (empty `BillingPeriod`) is handled by a default fallback at read time, not a bulk migration script.
