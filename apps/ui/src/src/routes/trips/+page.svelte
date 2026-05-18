<script lang="ts">
import { gql, queryStore } from "@urql/svelte";
import { onMount } from "svelte";
import { page } from "$app/stores";
import { paymentsUrql } from "$lib/stores/urql";
import { nav } from "$lib/stores/nav";
import type { Trip } from "$lib/types";
import Trips from "$lib/trips.svelte";
import { formatCurrencyAmount } from "../../utils/currency-formatter.util";
import { formatRelativeDate } from "../../utils/format-relative-date";

$: tripId = $page.url.searchParams.get('id');

// List page state
let showCreateForm = false;
let createTitle = '';
let createStart = '';
let createEnd = '';
let createError = '';
let createLoading = false;

// Detail page state
let showEditForm = false;
let editTitle = '';
let editStart = '';
let editEnd = '';
let editError = '';
let editLoading = false;

let loadingMore = false;
let endCursor: string | null = null;
let hasNextPage = true;
let extraTransactions: any[] = [];
let initialised = false;

onMount(() => {
  nav.update((prev) => ({ ...prev, isOpen: true }));
});

$: tripsQuery = queryStore({
  client: $paymentsUrql,
  query: gql`
    query {
      trips {
        id
        title
        startDate
        endDate
      }
    }
  `,
});

$: allTrips = ($tripsQuery.data?.trips ?? []) as Trip[];
$: currentTrip = allTrips.find((t) => t.id === tripId);

$: if (currentTrip && showEditForm) {
  editTitle = currentTrip.title;
  editStart = currentTrip.startDate ? currentTrip.startDate.split('T')[0] : '';
  editEnd = currentTrip.endDate ? currentTrip.endDate.split('T')[0] : '';
}

// Reset detail state when navigating to a different trip
$: if (tripId) {
  extraTransactions = [];
  initialised = false;
  endCursor = null;
  hasNextPage = true;
}

const TRIP_TRANSACTIONS_QUERY = gql`
  query TripTransactions {
    trips {
      id
      transactions(first: 15) {
        nodes {
          id
          merchant
          amount
          normalizedAmount
          currency
          paidAt
        }
        pageInfo {
          endCursor
          hasNextPage
        }
      }
    }
  }
`;

$: tripTransactionsQuery = tripId
  ? queryStore({ client: $paymentsUrql, query: TRIP_TRANSACTIONS_QUERY })
  : null;

$: currentTripData = $tripTransactionsQuery?.data
  ? ($tripTransactionsQuery.data.trips as any[]).find((t: any) => t.id === tripId)
  : null;

$: initialTransactions = currentTripData?.transactions?.nodes ?? [];

$: {
  if (currentTripData?.transactions?.pageInfo && !initialised) {
    endCursor = currentTripData.transactions.pageInfo.endCursor ?? null;
    hasNextPage = currentTripData.transactions.pageInfo.hasNextPage ?? false;
    initialised = true;
  }
}

$: allTransactions = [...initialTransactions, ...extraTransactions];

async function loadMore() {
  if (loadingMore || !endCursor) return;
  loadingMore = true;
  try {
    const result = await $paymentsUrql.query(
      `query TripTransactionsMore($after: String) {
        trips {
          id
          transactions(first: 15, after: $after) {
            nodes {
              id
              merchant
              amount
              normalizedAmount
              currency
              paidAt
            }
            pageInfo { endCursor hasNextPage }
          }
        }
      }`,
      { after: endCursor },
    ).toPromise();

    const tripData = (result?.data?.trips as any[])?.find((t: any) => t.id === tripId);
    if (tripData) {
      const existingIds = new Set(allTransactions.map((t: any) => t.id));
      const newNodes = (tripData.transactions?.nodes ?? []).filter((t: any) => !existingIds.has(t.id));
      extraTransactions = [...extraTransactions, ...newNodes];
      endCursor = tripData.transactions?.pageInfo?.endCursor ?? null;
      hasNextPage = tripData.transactions?.pageInfo?.hasNextPage ?? false;
    }
  } finally {
    loadingMore = false;
  }
}

function hasOverlap(start: string, end: string, excludeId: string | null): Trip[] {
  if (!start || !end) return [];
  const s = new Date(start);
  const e = new Date(end);
  return allTrips.filter((t) => {
    if (t.id === excludeId || !t.startDate || !t.endDate) return false;
    return s < new Date(t.endDate) && e > new Date(t.startDate);
  });
}

function refreshTrips() {
  tripsQuery = queryStore({
    client: $paymentsUrql,
    query: gql`query { trips { id title startDate endDate } }`,
  });
}

async function submitCreate() {
  createError = '';
  if (!createTitle.trim()) { createError = 'Title is required.'; return; }

  const conflicts = hasOverlap(createStart, createEnd, null);
  if (conflicts.length > 0) {
    createError = `Overlaps with: ${conflicts.map((t) => t.title).join(', ')}`;
    return;
  }

  createLoading = true;
  try {
    const r = await $paymentsUrql.mutation(
      gql`mutation TripCreate($input: TripCreateDTOInput!) {
        tripCreate(input: $input) { id title startDate endDate }
      }`,
      { input: { title: createTitle, startDate: createStart || null, endDate: createEnd || null } },
    ).toPromise();
    if (r.error) { createError = r.error.message; return; }
    showCreateForm = false;
    createTitle = ''; createStart = ''; createEnd = '';
    refreshTrips();
  } finally {
    createLoading = false;
  }
}

async function submitEdit() {
  if (!tripId) return;
  editError = '';
  if (!editTitle.trim()) { editError = 'Title is required.'; return; }

  const conflicts = hasOverlap(editStart, editEnd, tripId);
  if (conflicts.length > 0) {
    editError = `Overlaps with: ${conflicts.map((t) => t.title).join(', ')}`;
    return;
  }

  editLoading = true;
  try {
    const r = await $paymentsUrql.mutation(
      gql`mutation TripUpdate($id: String!, $input: TripUpdateDTOInput!) {
        tripUpdate(id: $id, input: $input) { id title startDate endDate }
      }`,
      { id: tripId, input: { title: editTitle, startDate: editStart || null, endDate: editEnd || null } },
    ).toPromise();
    if (r.error) { editError = r.error.message; return; }
    showEditForm = false;
    refreshTrips();
  } finally {
    editLoading = false;
  }
}
</script>

{#if tripId}
  <!-- Trip detail view -->
  <section>
    {#if $tripsQuery.fetching && !currentTrip}
      <p class="loading">Loading...</p>
    {:else}
    <div class="trip-header">
      <a href="/trips" class="back">← Trips</a>
      <div class="trip-title-row">
        <h1>{currentTrip?.title ?? '...'}</h1>
        <button class="edit-btn" on:click={() => { showEditForm = !showEditForm; editError = ''; }}>
          {showEditForm ? 'Cancel' : 'Edit'}
        </button>
      </div>
      {#if currentTrip?.startDate && currentTrip?.endDate}
        <p class="date-range">
          {new Date(currentTrip.startDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
          –
          {new Date(currentTrip.endDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
        </p>
      {/if}
    </div>

    {#if showEditForm}
      <form on:submit|preventDefault={submitEdit} class="edit-form">
        <label>
          Title
          <input type="text" bind:value={editTitle} />
        </label>
        <label>
          Start Date
          <input type="date" bind:value={editStart} />
        </label>
        <label>
          End Date
          <input type="date" bind:value={editEnd} />
        </label>
        {#if editError}
          <p class="error">{editError}</p>
        {/if}
        <button type="submit" disabled={editLoading}>{editLoading ? 'Saving...' : 'Save'}</button>
      </form>
    {/if}

    <div class="transactions-section">
      {#if $tripTransactionsQuery?.fetching && allTransactions.length === 0}
        <p class="loading">Loading transactions...</p>
      {:else if allTransactions.length === 0}
        <p class="empty">No transactions found for this trip.</p>
      {:else}
        <ul class="transaction-list">
          {#each allTransactions as txn (txn.id)}
            <li class="transaction-item">
              <a href={`/transaction?id=${txn.id}`} class="transaction-link">
                <div class="txn-left">
                  <span class="txn-merchant">{txn.merchant ?? 'Unknown'}</span>
                  <span class="txn-date">{txn.paidAt ? formatRelativeDate(new Date(txn.paidAt)) : ''}</span>
                </div>
                <div class="txn-amount-group">
                  <span class="txn-amount">{formatCurrencyAmount(txn.amount, txn.currency)}</span>
                  {#if txn.normalizedAmount != null && txn.normalizedAmount !== txn.amount}
                    <span class="txn-normalized">({formatCurrencyAmount(txn.normalizedAmount, "INR")})</span>
                  {/if}
                </div>
              </a>
            </li>
          {/each}
        </ul>

        {#if hasNextPage}
          <button class="load-more" on:click={loadMore} disabled={loadingMore}>
            {loadingMore ? 'Loading...' : 'Load more'}
          </button>
        {/if}
      {/if}
    </div>
    {/if}
  </section>
{:else}
  <!-- Trip list view -->
  <section>
    <Trips trips={allTrips} fetching={$tripsQuery.fetching} onNewTrip={() => (showCreateForm = true)} />

    {#if showCreateForm}
      <div class="create-area">
        <form on:submit|preventDefault={submitCreate}>
          <h2>New Trip</h2>
          <label>
            Title
            <input type="text" bind:value={createTitle} placeholder="Europe 2025" />
          </label>
          <label>
            Start Date
            <input type="date" bind:value={createStart} />
          </label>
          <label>
            End Date
            <input type="date" bind:value={createEnd} />
          </label>
          {#if createError}
            <p class="error">{createError}</p>
          {/if}
          <div class="form-actions">
            <button type="submit" disabled={createLoading}>{createLoading ? 'Saving...' : 'Create'}</button>
            <button type="button" on:click={() => { showCreateForm = false; createError = ''; }}>Cancel</button>
          </div>
        </form>
      </div>
    {/if}
  </section>
{/if}

<style>
  section {
    padding: 1rem;
  }

  /* List view */
  .create-area {
    margin-top: 1rem;
  }

  /* Detail view */
  .trip-header {
    margin-bottom: 1.25rem;
  }

  .back {
    font-size: 0.85rem;
    color: var(--color-neutral-content);
    display: inline-block;
    margin-bottom: 0.5rem;
  }

  .trip-title-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .trip-title-row h1 {
    margin: 0;
  }

  .edit-btn {
    background: transparent;
    border: 1px solid var(--color-base-300);
    border-radius: 6px;
    color: var(--color-neutral-content);
    font-size: 0.8rem;
    padding: 0.3rem 0.75rem;
  }

  .date-range {
    margin: 0.35rem 0 0;
    font-size: 0.85rem;
    opacity: 0.6;
  }

  .edit-form {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    max-width: 360px;
    margin-bottom: 1.5rem;
    padding-bottom: 1.5rem;
    border-bottom: 1px solid var(--color-base-300);
  }

  label {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    font-size: 0.9rem;
  }

  input {
    padding: 0.4rem 0.6rem;
    font-size: 1rem;
    border: 1px solid var(--color-base-300);
    border-radius: 4px;
    background: var(--color-base-200);
    color: var(--color-base-content);
  }

  .error {
    color: var(--color-error);
    font-size: 0.85rem;
    margin: 0;
  }

  .transactions-section {
    margin-top: 0.5rem;
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .transaction-list {
    list-style: none;
    padding: 0;
    margin: 0;
    width: 100%;
  }

  .transaction-item {
    border-bottom: 1px solid var(--color-base-200);
  }

  .transaction-item:last-child {
    border-bottom: none;
  }

  .transaction-link {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 0;
    text-decoration: none;
    color: inherit;
  }

  .txn-left {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
  }

  .txn-merchant {
    font-size: 0.9rem;
    font-weight: 500;
    color: var(--color-base-content);
  }

  .txn-date {
    font-size: 0.75rem;
    color: var(--color-neutral-content);
  }

  .txn-amount-group {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.1rem;
  }

  .txn-amount {
    font-family: "JetBrains Mono", monospace;
    font-size: 0.9rem;
    font-weight: 600;
    white-space: nowrap;
    color: var(--color-base-content);
  }

  .txn-normalized {
    font-family: "JetBrains Mono", monospace;
    font-size: 0.75rem;
    color: var(--color-neutral-content);
    white-space: nowrap;
  }

  .load-more {
    display: block;
    width: 100%;
    margin-top: 1rem;
    padding: 0.6rem;
    background: transparent;
    border: 1px solid var(--color-base-300);
    border-radius: 6px;
    color: var(--color-neutral-content);
    font-size: 0.875rem;
  }

  .load-more:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .loading, .empty {
    opacity: 0.6;
    font-size: 0.875rem;
    margin-top: 0.5rem;
  }

  form {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    max-width: 360px;
    margin-top: 1rem;
  }

  .form-actions {
    display: flex;
    gap: 0.5rem;
  }
</style>
