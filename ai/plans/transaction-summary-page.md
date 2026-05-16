# Transaction Summary Page — Implementation Plan

## Context
Users need a summary/overview page showing aggregate transaction stats filterable by date range. The `TransactionStats` GraphQL query returns a generic array of `{ stat: string, value: string }` objects so the UI can render any stat without schema changes. A `scope` enum filter lets the UI request one group of stats at a time (parse status, tags, or currency).

## Current state (merged to `feat/payments-transaction-stats-query`)
- `TransactionStatDTO`, `TransactionStatsFiltersInput`, `ITransactionStatsService` — created
- `TransactionStatScope` enum — created (`Parse`, `Tags`, `Currency`)
- `TransactionStatsFiltersInput.Scope` — added
- `NocoDBClientService.GetGroupByAsync<T>` — added with error logging
- `NocoDBClientService.GetCountAsync` — added (hits `/count` endpoint)
- `TransactionStatsNocoDBService` — routes by scope; all three groups fire concurrently via `Task.WhenAll` when scope is null
- Parse scope: `GetGroupByAsync(ParseStatus)` → `total`, `completed`, `notStarted`, `pending`, `failed`
- Tags scope: `GetCountAsync` with `isnotblank`/`isblank` → `tagged`, `untagged`
- Currency scope: `GetGroupByAsync(Currency)` → `currency_{code}` per distinct value
- `Query.TransactionStats` field and DI registration — wired up
- ParseStatus values mapped: `ParsedV1/Parsed/OcrParsedV1` → completed, `Parsing` → pending, `FailedV1/OcrFailedV1` → failed

---

## Remaining work

### ~~1. Scope enum + filter field~~ ✓ Done
### ~~2. `GetCountAsync` in NocoDBClientService~~ ✓ Done
### ~~3. Expand `TransactionStatsNocoDBService`~~ ✓ Done

---

### 4. UI: `/transactions/summary` page

**New file:** `apps/ui/src/src/routes/transactions/summary/+page.svelte`

- Period selector buttons: Last 7 / 30 / 60 / 90 days — computes `fromDate` via `dayjs().subtract(N, 'day').toISOString()`
- Three separate `queryStore` calls (one per scope: `PARSE`, `TAGS`, `CURRENCY`) so each card group loads independently
- Render one stat card per `{ stat, value }` entry using existing DaisyUI/color tokens

**Modify `apps/ui/src/src/lib/Nav.svelte`** — add "Summary" nav link pointing to `/transactions/summary`

### GraphQL queries (UI)
```graphql
query ParseStats($fromDate: String) {
  transactionStats(filters: { fromDate: $fromDate, scope: PARSE }) { stat value }
}
query TagStats($fromDate: String) {
  transactionStats(filters: { fromDate: $fromDate, scope: TAGS }) { stat value }
}
query CurrencyStats($fromDate: String) {
  transactionStats(filters: { fromDate: $fromDate, scope: CURRENCY }) { stat value }
}
```

---

## Critical files

| File | Action |
|------|--------|
| `apps/payments/src/Services.Contracts/DTOs/TransactionStatScopeEnum.cs` | Create |
| `apps/payments/src/Services.Contracts/DTOs/TransactionStatsFiltersInput.cs` | Modify — add `Scope` |
| `apps/payments/src/NocoDB/NocoDBClientService.cs` | Modify — add `GetCountAsync` |
| `apps/payments/src/Services/TransactionStatsNocoDBService.cs` | Modify — add Tags + Currency scopes |
| `apps/ui/src/src/routes/transactions/summary/+page.svelte` | Create |
| `apps/ui/src/src/lib/Nav.svelte` | Modify |

**Reference implementations:**
- GroupBy pattern: `apps/payments/src/Services/TransactionStatsNocoDBService.cs`
- Count endpoint pattern: mirror `GetGroupByAsync` in `apps/payments/src/NocoDB/NocoDBClientService.cs`
- UI query pattern: `apps/ui/src/src/routes/transactions/+page.svelte`

---

## Verification

1. `cd apps/payments && dotnet build` — no errors
2. GraphQL (no scope — all stats):
```graphql
{ transactionStats(filters: { fromDate: "2025-01-01T00:00:00Z" }) { stat value } }
```
Expected shape:
```json
[
  { "stat": "total",        "value": "87" },
  { "stat": "completed",    "value": "54" },
  { "stat": "notStarted",   "value": "20" },
  { "stat": "pending",      "value": "10" },
  { "stat": "failed",       "value": "3"  },
  { "stat": "tagged",       "value": "60" },
  { "stat": "untagged",     "value": "27" },
  { "stat": "currency_INR", "value": "70" },
  { "stat": "currency_USD", "value": "17" }
]
```
3. GraphQL (scoped): `scope: PARSE` returns only parse stats, `scope: TAGS` returns only tagged/untagged, `scope: CURRENCY` returns only currency entries.
4. UI: navigate to `/transactions/summary`, toggle period buttons, verify each card group loads.
