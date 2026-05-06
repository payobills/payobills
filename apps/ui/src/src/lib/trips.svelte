<script lang="ts">
import type { Trip } from "./types";
import Card from "$lib/card.svelte";
import IdeaCard from "$lib/idea-card.svelte";
export let trips: Trip[];

function formatDateRange(trip: Trip): string {
  if (!trip.startDate || !trip.endDate) return '';
  const fmt = (d: string) => new Date(d).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
  return `${fmt(trip.startDate)} – ${fmt(trip.endDate)}`;
}
</script>

{#if trips}

<h1>Trips</h1>

<IdeaCard idea={`Going for a trip? Group your transactions together and manage them easily...`} />

{#if trips.length === 0}
  <p>Looks like you haven't created any trips yet. Create one below to start tracking expenses.</p>
{:else}
  <p>Transactions made on a trip can help you track your expenses in one place.</p>
  <section class='trip-cards'>
    {#each trips as trip (trip.id)}
      <a href={`trips?id=${trip.id}`}>
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
  a {
    text-decoration: none;
  }

  p {
    margin-top: 1rem;
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

  :global(:first-of-type(.trip-cards .card)) {
    margin-top: 0;
  }

  :global(:last-of-type(.trip-cards .card)) {
    margin-bottom: 0;
  }
</style>
