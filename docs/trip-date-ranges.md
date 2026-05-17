# Trip Date Ranges

## Context

Trips already exist in the system as `TransactionTag` — a tag you can attach to individual transactions. The tag appears on the transaction edit page, and the trips list page shows all transactions tagged with a given trip.

The missing piece: **no date range is attached to a trip**. You can't say "I was in Europe from May 1 to May 15" and then see all transactions that happened in that window — you'd have to tag each one manually. This feature adds start/end dates to trips, enabling two things:

1. A "trip view" that pulls transactions by date range (not just by manual tag)
2. A context hint for the SLM parser: when parsing a transaction whose date falls inside a trip's range, inject the trip context into the prompt to boost parsing confidence

---

## Goal

- Add `StartDate` / `EndDate` to trips
- UI for setting dates when creating or editing a trip
- Trip detail page shows all transactions within the date range (in addition to manually tagged ones)
- Transaction parser injects active-trip context when a transaction's date falls inside a trip window

---

## Data Model

Trips are currently stored as multi-select column options on the `Transactions` table in NocoDB — just strings. That format cannot hold metadata like dates.

A new **`Trips`** table in NocoDB is needed:

| Column | Type | Notes |
|---|---|---|
| `Id` | Auto-increment int | Primary key |
| `Title` | Text | Must match the tag string on Transactions (used as the link) |
| `StartDate` | Date | Inclusive |
| `EndDate` | Date | Inclusive |
| `CreatedAt` | DateTime | Auto |
| `UpdatedAt` | DateTime | Auto |

The existing `Tags` multi-select column on `Transactions` is unchanged. The `Title` in the `Trips` table is the join key — a transaction tagged "Europe 2025" links to the `Trips` row whose title is "Europe 2025". This keeps backwards compatibility and avoids migrating the tag column.

---

## Architecture

```
UI (SvelteKit)
  ├─ /trips            → list trips with date ranges
  ├─ /trips/new        → create trip (title + date pickers)
  ├─ /trips/:id/edit   → edit trip dates
  └─ /trips/:id        → show transactions in date range ∪ tagged transactions

GraphQL API (C# HotChocolate)
  ├─ Query: trips { id title startDate endDate }
  ├─ Mutation: tripCreate(input) → Trip
  └─ Mutation: tripUpdate(id, input) → Trip

NocoDB
  ├─ Trips table (new)
  └─ Transactions.Tags column (unchanged)

Transaction Parser (Rust)
  └─ trips_store.rs (new)
       → fetch active trips for a transaction date
       → inject trip context into SLM prompt
```

---

## Components

### 1. NocoDB — `Trips` table

Create the table via the NocoDB UI or API before wiring backend code. The `Title` column is the link to the existing tag system. If a trip has no date range yet (legacy data), `StartDate` and `EndDate` can be null — the row still acts as metadata for the tag.

### 2. Backend — C# service

**New DTO:**
```csharp
public record TripDTO(string Id, string Title, DateTime? StartDate, DateTime? EndDate);
public record TripCreateOrUpdateDTO(string Title, DateTime? StartDate, DateTime? EndDate);
```

**`TripsNocoDBService`** — mirrors the existing `TransactionsNocoDBService` pattern:
- `GetTripsAsync()` — fetch all rows from `Trips` table
- `GetTripByIdAsync(id)` — fetch single row
- `CreateTripAsync(input)` — insert row
- `UpdateTripAsync(id, input)` — patch row

**GraphQL additions in `Query.cs` / `Mutation.cs`:**
```csharp
// Query
public async Task<IEnumerable<TripDTO>> Trips([Service] TripsNocoDBService svc)
    => await svc.GetTripsAsync();

// Mutations
public async Task<TripDTO> TripCreate(TripCreateOrUpdateDTO input, [Service] TripsNocoDBService svc)
    => await svc.CreateTripAsync(input);

public async Task<TripDTO> TripUpdate(string id, TripCreateOrUpdateDTO input, [Service] TripsNocoDBService svc)
    => await svc.UpdateTripAsync(id, input);
```

**`transactions` query** — extend `TransactionFiltersInput` to support date-range filtering:
```csharp
public record TransactionFiltersInput(
    IEnumerable<string>? Tags,
    DateTime? FromDate,   // new
    DateTime? ToDate      // new
);
```

The NocoDB query for date range uses the existing `where` filter syntax with `BackDate` column.

### 3. Frontend — SvelteKit

**Type update:**
```typescript
export type Trip = {
  id: string;
  title: string;
  startDate: Date | null;
  endDate: Date | null;
};
```

**Trip list page (`/trips`)** — add date range display on trip cards:
```
Europe 2025
May 1 – May 15, 2025
```

**Trip create/edit form** — add two date inputs (`startDate`, `endDate`). Both optional so existing trips without dates keep working.

When creating a trip, the UI checks existing trips for date-range overlap before submitting. Same-day boundaries are allowed — a trip ending May 15 and a new trip starting May 15 is fine (morning/evening split). Exact date overlap triggers a warning inline on the form listing the conflicting trip(s); the user cannot submit until the conflict is resolved. Editing an existing trip uses the same overlap check but excludes the trip being edited from the comparison.

The edit form is accessed via an **Edit button on the trip detail page** — no separate `/trips/:id/edit` route.

**Trip detail page (`/trips/:id`)** — change the query strategy:
- If trip has `startDate`/`endDate`: query `transactions(filters: { fromDate, toDate })` and merge with tagged transactions, deduplicating by id
- If no date range: fall back to current tag-only query

Show a visual indicator on each transaction: "in date range" vs "manually tagged" vs both.

**Tag auto-suggestion on the transaction edit page** — when a transaction's `paidAt` / `backDate` falls inside one or more trip date ranges, surface those trips as suggested tags at the top of the available-tags list, visually distinguished (e.g. a small calendar badge). If the user adds the suggested tag, it moves to the added-tags section as normal. This handles the case where multiple trips match (e.g. a one-day overlap at a boundary) by surfacing all matches.

### 4. Transaction Parser — Rust

New file **`trips_store.rs`** in `apps/transaction-parser/src/payobills/`:

```rust
pub struct TripContext {
    pub title: String,
    pub start_date: NaiveDate,
    pub end_date: NaiveDate,
}

pub async fn get_active_trips_for_date(
    client: &NocoDBClient,
    date: NaiveDate,
) -> anyhow::Result<Vec<TripContext>>
```

This fetches all trips from NocoDB and filters to those whose `[StartDate, EndDate]` window contains `date`. If the transaction's `BackDate` is not yet parsed, this step is skipped.

**Prompt injection in `slm_client.rs`:**

The existing prompt asks the SLM to extract regex patterns from raw SMS text. When trip context is available, append a note:

```
Additional context: The user was on a trip titled "{title}" during this transaction 
(trip dates: {start_date} to {end_date}). Use this to resolve ambiguous merchant 
locations or currencies.
```

This is appended to the existing `generate_regex` prompt. The SLM receives it as soft context — it can use it to bias currency selection (e.g. EUR over INR for a Europe trip) without changing the output format (the regex + fields JSON stays the same).

---

## Implementation Sequence

1. **Create `Trips` table in NocoDB** — manually, with Id/Title/StartDate/EndDate columns
2. **Backend: `TripsNocoDBService` + DTOs** — CRUD against the new table
3. **Backend: GraphQL `trips` query + `tripCreate`/`tripUpdate` mutations**
4. **Backend: extend `TransactionFiltersInput`** with `fromDate`/`toDate`, wire to NocoDB query
5. **Frontend: update `Trip` type** — add startDate/endDate fields
6. **Frontend: trip create/edit form** — date pickers, call `tripCreate`/`tripUpdate`
7. **Frontend: trip list page** — display date range on cards
8. **Frontend: trip detail page** — merge date-range query with tag query
9. **Parser: `trips_store.rs`** — fetch active trips for a date
10. **Parser: inject trip context in `slm_client.rs`** — append context note to prompt when trips found

Steps 1–4 can be done in one PR. Steps 5–8 in a second. Step 9–10 as a third, since it depends on the NocoDB trips table being populated with real data.

---

