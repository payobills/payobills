<script lang="ts">
  import type { Trip } from "./types";
  import Card from '$lib/card.svelte';
  export let trips: Trip[];
</script>

{#if trips}

<h1>Trips</h1>

<section class='trip-cards'>
{#if trips.length === 0}
  <p>Looks like you haven't created any trips yet. Tag any transaction to a trip so it shows up here...</p>
{:else}
  {#each trips as trip (trip.id)}
    <a href={`trips?id=${trip.id}&title=${trip.title}`}>
      <Card>
        <h2>{trip.title}</h2>
      </Card>
    </a>
  {/each}
{/if}
</section>

{/if}

<style>
  h1 {
    margin-top: 2rem;
    margin-bottom: 0;
  }

  :global(.trip-cards .card) {
    margin: 1rem 0;
  }

  :global(:first-of-type(.trip-cards .card)) {
    margin-top: 0;
  }

  :global(:last-of-type(.trip-cards .card)) {
    margin-bottom: 0;
  }

</style>
