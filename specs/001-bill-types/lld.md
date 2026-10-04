# Low-Level Design: Bill Types

**Date**: 2026-06-13 | **Plan**: [plan.md](plan.md) | **HLD**: [hld.md](hld.md)

## Data Model Changes

No data model changes. `BillingPeriod` already exists in the `bills` table — no new columns, no schema migrations required.

**Validation rules**:
- `BillingPeriod` MUST be one of: `monthly`, `yearly`, `prepaid`.
- If omitted on create, defaults to `"monthly"`.
- Write-once: set at creation and never updated thereafter.
- When `prepaid`: `BillingDate`, `PayByDate`, and `LatePayByDate` are optional.

## DTO Changes

### `bills` service

**`CreateBillDTO`** — add field:
- `BillingPeriod` (string, default `"monthly"`)

**`Bill` model** — no change (`BillingPeriod` already present).

**`BillDTO`** — no change (`BillingPeriod` already present and mapped).

**`BillsMappingProfile`** — no explicit mapping needed. AutoMapper resolves `BillingPeriod` by name convention from `CreateBillDTO` to `Bill`. Verify in tests.

## Frontend Changes

### Add Bill page (`routes/bills/add/+page.svelte`)

**What changes**: Add billing type selection to the add bill form.

**Component decision**:
- Build a new `BillingTypePicker` inline component (three pill/toggle buttons: **monthly** | **yearly** | **prepaid**).
- Default selection: `monthly`.
- Wire selected value into the `addBill` mutation as `billingPeriod`.
- When `prepaid` is selected, hide the `BillingDate` and `PayByDate` inputs (they are not required).

### Bills list and detail page (`routes/bills/+page.svelte`)

**What changes**: Display billing type on each bill in the list and in the detail/edit view.

**Component decision**:
- Build a new `BillingTypeBadge` inline component (read-only pill/badge).
- Reuse for both the list row and the detail view — no edit control rendered.
- Add `billingPeriod` to the `bills` query and `billById` query selection sets.
- Fallback: if `billingPeriod` is null or empty, display `"monthly"`.
