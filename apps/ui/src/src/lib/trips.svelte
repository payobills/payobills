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
    <Card>
      <h2>{trip.title}</h2>
    </Card>
  {/each}
{/if}
</section>

{/if}

<style>
  .trip-cards > :global(.card) {
    margin: 1rem 0;
  }

  .trip-cards > :global(.card):first-of-type {
    margin-top: 0;
  }

  .trip-cards > :global(.card):last-of-type {
    margin-bottom: 0;
  }

</style>
