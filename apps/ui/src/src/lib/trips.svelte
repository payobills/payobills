<script lang="ts">
import type { Trip } from "./types";
import Card from "$lib/card.svelte";
export let trips: Trip[];
export let onNewTrip: () => void;
export let fetching: boolean = false;

function formatDateRange(trip: Trip): string {
  if (!trip.startDate || !trip.endDate) return '';
  const fmt = (d: string) => new Date(d).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
  return `${fmt(trip.startDate)} – ${fmt(trip.endDate)}`;
}
</script>

{#if trips}

<div class="trips-header">
  <h1>Trips</h1>
  <button class="new-trip-btn" on:click={onNewTrip}>+ New Trip</button>
</div>

{#if fetching}
  <p>Loading...</p>
{:else if trips.length === 0}
  <p>No trips yet.</p>
{:else}
  <section class='trip-cards'>
    {#each trips as trip (trip.id)}
      <a href={`/trips/${trip.id}`}>
        <Card>
          <h2>{trip.title}</h2>
          {#if trip.startDate && trip.endDate}
            <p class="date-range">{formatDateRange(trip)}</p>
          {/if}
        </Card>
      </a>
    {/each}
  </section>
{/if}

{/if}

<style>
  .trips-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding-bottom: 0.75rem;
  }

  .new-trip-btn {
    background: transparent;
    border: 1px solid var(--color-base-300);
    border-radius: 6px;
    color: var(--color-primary);
    padding: 0.4rem 0.875rem;
    font-size: 0.85rem;
  }

  a {
    text-decoration: none;
  }

  h2 {
    margin: 0;
  }

  h1 {
    margin-bottom: 0;
  }

  .date-range {
    margin: 0.25rem 0 0;
    font-size: 0.85rem;
    opacity: 0.65;
  }
</style>
