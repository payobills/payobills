<script lang="ts">
import { gql, queryStore } from "@urql/svelte";
import { onMount } from "svelte";
import { paymentsUrql } from "$lib/stores/urql";
import { nav } from "$lib/stores/nav";
import type { Trip } from "$lib/types";
import Trips from "$lib/trips.svelte";

let showCreateForm = false;
let createTitle = '';
let createStart = '';
let createEnd = '';
let createError = '';
let createLoading = false;

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

function hasOverlap(start: string, end: string, trips: Trip[]): Trip[] {
  if (!start || !end) return [];
  const s = new Date(start);
  const e = new Date(end);
  return trips.filter((t) => {
    if (!t.startDate || !t.endDate) return false;
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
</script>

<section>
  <Trips trips={allTrips} onNewTrip={() => (showCreateForm = true)} />

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

<style>
  section {
    padding: 1rem;
  }

  .create-area {
    margin-top: 1rem;
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
    border: 1px solid var(--color-base-300);
    border-radius: 4px;
    background: var(--color-base-200);
    color: var(--color-base-content);
  }

  .form-actions {
    display: flex;
    gap: 0.5rem;
  }

  .error {
    color: var(--color-error);
    font-size: 0.85rem;
    margin: 0;
  }
</style>
