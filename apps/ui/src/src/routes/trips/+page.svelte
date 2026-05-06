<script lang="ts">
import { gql, queryStore, mutationStore, getContextClient } from "@urql/svelte";
import { onMount } from "svelte";
import { paymentsUrql } from "$lib/stores/urql";
import type { Trip } from "$lib/types";
import RecentTransactions from "../../lib/recent-transactions.svelte";
import Trips from "$lib/trips.svelte";

let tripId: string | undefined;
let showCreateForm = false;
let showEditForm = false;

let createTitle = '';
let createStart = '';
let createEnd = '';
let createError = '';
let createLoading = false;

let editTitle = '';
let editStart = '';
let editEnd = '';
let editError = '';
let editLoading = false;

onMount(() => {
  const id = new URLSearchParams(window.location.search)?.get("id");
  tripId = id ?? undefined;
});

const client = getContextClient();

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
$: currentTrip = tripId ? allTrips.find((t) => t.id === tripId) : undefined;

$: if (currentTrip && showEditForm) {
  editTitle = currentTrip.title;
  editStart = currentTrip.startDate ? currentTrip.startDate.split('T')[0] : '';
  editEnd = currentTrip.endDate ? currentTrip.endDate.split('T')[0] : '';
}

$: transactionsQuery = currentTrip
  ? queryStore({
      client: $paymentsUrql,
      query: gql`
        query TransactionsForTrip($tags: [String!]!) {
          transactions(filters: { tags: $tags }, first: 50) {
            nodes {
              id
              amount
              merchant
              createdAt
              paidAt
              updatedAt
              tags
            }
          }
        }
      `,
      variables: { tags: [currentTrip.title] },
    })
  : null;

function hasOverlap(start: string, end: string, trips: Trip[], excludeId?: string): Trip[] {
  if (!start || !end) return [];
  const s = new Date(start);
  const e = new Date(end);
  return trips.filter((t) => {
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

  const conflicts = hasOverlap(createStart, createEnd, allTrips);
  if (conflicts.length > 0) {
    createError = `Overlaps with: ${conflicts.map((t) => t.title).join(', ')}`;
    return;
  }

  createLoading = true;
  mutationStore({
    client,
    query: gql`
      mutation TripCreate($input: TripCreateDTOInput!) {
        tripCreate(input: $input) { id title startDate endDate }
      }
    `,
    variables: { input: { title: createTitle, startDate: createStart || null, endDate: createEnd || null } },
  }).subscribe((r) => {
    if (r.fetching) return;
    createLoading = false;
    if (r.error) { createError = r.error.message; return; }
    showCreateForm = false;
    createTitle = ''; createStart = ''; createEnd = '';
    refreshTrips();
  });
}

async function submitEdit() {
  if (!tripId) return;
  editError = '';
  if (!editTitle.trim()) { editError = 'Title is required.'; return; }

  const conflicts = hasOverlap(editStart, editEnd, allTrips, tripId);
  if (conflicts.length > 0) {
    editError = `Overlaps with: ${conflicts.map((t) => t.title).join(', ')}`;
    return;
  }

  editLoading = true;
  mutationStore({
    client,
    query: gql`
      mutation TripUpdate($id: String!, $input: TripUpdateDTOInput!) {
        tripUpdate(id: $id, input: $input) { id title startDate endDate }
      }
    `,
    variables: { id: tripId, input: { title: editTitle, startDate: editStart || null, endDate: editEnd || null } },
  }).subscribe((r) => {
    if (r.fetching) return;
    editLoading = false;
    if (r.error) { editError = r.error.message; return; }
    showEditForm = false;
    refreshTrips();
  });
}
</script>

<section>
  {#if !tripId}

    <Trips trips={allTrips} />

    <div class="create-area">
      {#if !showCreateForm}
        <button on:click={() => (showCreateForm = true)}>+ New Trip</button>
      {:else}
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
      {/if}
    </div>

  {:else}

    <div class="trip-header">
      <a href="/trips" class="back">← Trips</a>
      <div class="trip-title-row">
        <h1>{currentTrip?.title ?? '...'}</h1>
        <button on:click={() => { showEditForm = !showEditForm; editError = ''; }}>
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

    <RecentTransactions
      title=""
      viewType={'all'}
      showAllTransactions={true}
      showGraph={false}
      showViewAllCTA={false}
      transactions={$transactionsQuery?.data?.transactions?.nodes ?? []}
    />

  {/if}
</section>

<style>
  section {
    padding: 1rem;
  }

  .create-area {
    margin-top: 1.5rem;
  }

  form {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    max-width: 360px;
    margin-top: 1rem;
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
    border: 1px solid #ccc;
    border-radius: 4px;
  }

  .form-actions {
    display: flex;
    gap: 0.5rem;
  }

  .error {
    color: red;
    font-size: 0.85rem;
    margin: 0;
  }

  .trip-header {
    margin-bottom: 1rem;
  }

  .trip-title-row {
    display: flex;
    align-items: center;
    gap: 1rem;
  }

  .trip-title-row h1 {
    margin: 0;
  }

  .date-range {
    margin: 0.25rem 0 0;
    font-size: 0.9rem;
    opacity: 0.65;
  }

  .back {
    font-size: 0.9rem;
    color: inherit;
    display: inline-block;
    margin-bottom: 0.25rem;
  }

  .edit-form {
    margin-bottom: 1.5rem;
    padding-bottom: 1.5rem;
    border-bottom: 1px solid #eee;
  }
</style>
