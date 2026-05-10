# International Transactions — INR Normalization via n8n

## Context

The transaction-parser already converts foreign-currency transactions to INR using OpenExchangeRates and stores `normalized_amount`. However, this only runs during parsing; historical transactions or re-parse scenarios may have missing or stale values. We want a dedicated batch n8n workflow to ensure all foreign-currency transactions have a correctly computed `normalized_amount` in INR.

Additionally, `normalized_amount` is not exposed via GraphQL today, so the UI cannot display it.

---

## What Already Exists (Do Not Duplicate)

- `normalized_amount` field in NocoDB, C# Transaction model, and TransactionDTO
- `currencies_historical` table in NocoDB — historical rates keyed by date, relative to USD
- `currency_sync.rs` — `fetch_and_store_rates()` and `get_conversion_rates()` functions (internal only)
- Conversion formula: `amount / rate(source→USD) * rate(INR→USD)` (already in `transaction_parser.rs`)

---

## Implementation Plan

### Step 1 — Expose currency_sync as an HTTP endpoint

**Problem:** `currency_sync` functions are only called internally. n8n cannot trigger rate fetching without an HTTP endpoint.

**Change:** Add a new route to `apps/transaction-parser/src/api/mod.rs`:

```
POST /api/currency-sync
Body: { "date": "YYYY-MM-DD" }   // optional; defaults to today
```

This handler calls `fetch_and_store_rates(date)` and returns the stored rates. If the date's rates already exist in NocoDB (`current == null` = finalized), it returns them without re-fetching.

**Files:**
- `apps/transaction-parser/src/api/mod.rs` — add route
- `apps/transaction-parser/src/payobills/currency_sync.rs` — `fetch_and_store_rates` is already callable with a date param

---

### Step 2 — n8n Workflow A: Batch Creator

**File:** `docs/n8n-batch-creator-workflow.json`

**Logic:**
1. **Trigger:** Manual (or scheduled nightly)
2. **Fetch all transactions** from NocoDB where `Currency != "INR"` and `ParseStatus` in `[ParsedV1, ParsedBySLM]`
3. **Group by source currency** — minimises rate lookups per batch
4. **Chunk each group** into batches of 100 transaction IDs
5. **For each batch:** call n8n Workflow B via webhook with `{ "transaction_ids": [...], "source_currency": "USD" }`

---

### Step 3 — n8n Workflow B: Batch Processor

**File:** `docs/n8n-batch-processor-workflow.json`

**Trigger:** Webhook (called by Workflow A)

**Input:** `{ "transaction_ids": [1, 2, ...], "source_currency": "USD" }`

**Logic per transaction:**
1. Fetch transaction from NocoDB (get `Amount`, `BackDate`, `Currency`)
2. Check if historical rate for `BackDate` exists in `currencies_historical`
   - If missing: call `POST /api/currency-sync` on transaction-parser with `{ "date": BackDate }`
3. Calculate: `normalized_amount = amount / rate[source_currency] * rate["INR"]`
4. PATCH transaction in NocoDB to set `NormalizedAmount`
5. Collect results: updated count, skipped (no rate found), errors

---

### Step 4 — Expose `normalized_amount` in GraphQL (payments service)

**Problem:** `NormalizedAmount` exists in the C# model (`Transaction.cs`) and DTO (`TransactionDTO.cs`) but is absent from the published supergraph schema.

**Change:** In `apps/payments`, ensure HotChocolate includes `NormalizedAmount` on the `TransactionDTO` type. With HotChocolate + Apollo Federation, check for any explicit type descriptors in `apps/payments/src/API/` that may be excluding it.

After exposing it, the supergraph SDL at `repos/payobills-argo-config/playground/supergraph.graphql` must be republished via the normal schema publish pipeline.

**Files:**
- `apps/payments/src/Services.Contracts/DTOs/TransactionDTO.cs` — verify field is included
- `apps/payments/src/API/` — check HotChocolate type descriptors
- `repos/payobills-argo-config/playground/supergraph.graphql` — republish after schema change

---

### Step 5 — UI display (minimal)

In `apps/ui`, query `normalizedAmount` from GraphQL on the transaction list/detail views and display it alongside the source currency. No calculation in the UI — read the pre-computed value.

---

## Files to Create / Modify

| File | Action | Purpose |
|------|--------|---------|
| `docs/international-currency-normalization.md` | Create | This document |
| `docs/n8n-batch-creator-workflow.json` | Create | n8n Workflow A (batch creator) |
| `docs/n8n-batch-processor-workflow.json` | Create | n8n Workflow B (batch processor webhook) |
| `apps/transaction-parser/src/api/mod.rs` | Modify | Add `POST /api/currency-sync` route |
| `apps/payments/src/API/` | Modify | Expose `NormalizedAmount` in HotChocolate schema |
| `repos/payobills-argo-config/playground/supergraph.graphql` | Update | Republish supergraph SDL |
| `apps/ui/src` | Modify | Display `normalizedAmount` from GraphQL |

---

## Verification

1. Call `POST /api/currency-sync` with a historical date → confirm rates stored in NocoDB `currencies_historical`
2. Run Workflow A manually → confirm it creates batches and triggers Workflow B for each
3. Check NocoDB transactions after Workflow B runs → `NormalizedAmount` populated for foreign-currency rows
4. Spot-check one transaction: manually compute `amount / rate[src] * rate[INR]` using stored rates and compare
5. Query `{ transactions { normalizedAmount currency amount } }` via GraphQL playground → field appears
6. Load UI transaction list → `normalizedAmount` displayed in INR
