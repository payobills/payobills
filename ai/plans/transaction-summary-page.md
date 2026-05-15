# Transaction Summary Page — Implementation Plan

## Context
Users currently only have a monthly view of transactions. We need a summary/overview page that shows aggregate stats (total, parsed, pending, failed) filterable by date range. The UI will offer preset windows (last 7/30/60/90 days) calculated client-side as ISO date strings. This is implemented service-by-service.

---

## Scope

| Service | Change |
|---------|--------|
| `apps/payments` (C#/.NET GraphQL) | Add `TransactionStats` GraphQL query with `fromDate` filter |
| `apps/ui` (SvelteKit) | Add `/transactions/summary` page |

`apps/transaction-parser` (Rust) — no changes for this phase.

---

## Phase 1 — Payments service: `TransactionStats` query

**New files:**
- `apps/payments/src/Services.Contracts/DTOs/TransactionStatsDTO.cs`
- `apps/payments/src/Services.Contracts/DTOs/TransactionStatsFiltersInput.cs`
- `apps/payments/src/Services.Contracts/ITransactionStatsService.cs`
- `apps/payments/src/Services/TransactionStatsNocoDBService.cs`

**Modify:**
- `apps/payments/src/Services/Query.cs` — add `TransactionStats` field
- `apps/payments/src/Program.cs` — register the new service

### DTO shapes

```csharp
// TransactionStatsFiltersInput.cs
public class TransactionStatsFiltersInput {
    public string? FromDate { get; set; }  // ISO date string, e.g. "2025-04-15"
}

// TransactionStatsDTO.cs
public record TransactionStatsDTO(
    int Total,
    int NotStarted,
    int Pending,
    int Completed,
    int Failed
);
```

### Service implementation

Follow the exact same NocoDB URL-param filter pattern used in `TransactionsNocoDBService.GetTransactionsAsync`:

- Build `where` clause with `(PaidAt,gte,{filters.FromDate})` when `FromDate` is not null/empty.
- Fetch all matching rows with `fields=ParseStatus` only, then group in memory to count by `ParseStatus`.

### Query registration

```csharp
// Query.cs — new field
public async Task<TransactionStatsDTO> TransactionStats(
    [Service] ITransactionStatsService statsService,
    TransactionStatsFiltersInput? filters = null)
  => await statsService.GetTransactionStatsAsync(filters);
```

---

## Phase 2 — UI: `/transactions/summary` page

**New file:** `apps/ui/src/src/routes/transactions/summary/+page.svelte`

**Modify:** `apps/ui/src/src/lib/Nav.svelte` — add "Summary" nav link

### Page layout

- **Period selector**: buttons for Last 7 / 30 / 60 / 90 days — each sets `fromDate` as `dayjs().subtract(N, 'day').toISOString()`
- **Stat cards** (DaisyUI card + existing color tokens):
  - Total | Completed | Not Started | Pending | Failed
- **Transaction list**: reuse existing `Transactions` query with `parseStatus` field showing a status badge per row

### GraphQL query

```graphql
query ($fromDate: String) {
  transactionStats(filters: { fromDate: $fromDate }) {
    total
    notStarted
    pending
    completed
    failed
  }
}
```

---

## Implementation Order

1. **Branch**: `feature/transaction-summary-page` off `main` ✅
2. **payments**: DTO + filter input + service interface + NocoDB service + Query field + DI registration
3. **UI**: summary page with period selector + stat cards + nav link

---

## Verification

- `cd apps/payments && dotnet build` — no errors
- GraphQL: `{ transactionStats(filters: { fromDate: "2025-01-01" }) { total notStarted pending completed failed } }`
- UI: navigate to `/transactions/summary`, toggle period buttons, verify card counts update
