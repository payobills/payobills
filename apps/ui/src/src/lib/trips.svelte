<script lang="ts">
  import type { Trip } from "./types";
  import Card from '$lib/card.svelte';
  import IdeaCard from "$lib/idea-card.svelte";
  export let trips: Trip[];
</script>

{#if trips}

<h1>Trips</h1>


      <IdeaCard
        idea={`Going for a trip? Group your transactions together and manage them easily...`}
      />

{#if trips.length === 0}
  <p>Looks like you haven't created any trips yet. Tag any transaction to a trip so it shows up here...</p>
{:else}
  <p>Transactions made on a trip can help you track your expenses in one place.</p>
<section class='trip-cards'>
  {#each trips as trip (trip.id)}
    <a href={`trips?id=${trip.id}&title=${trip.title}`}>
      <Card>
        <h2>{trip.title}</h2>
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

  :global(:first-of-type(.trip-cards .card)) {
    margin-top: 0;
  }

  :global(:last-of-type(.trip-cards .card)) {
    margin-bottom: 0;
  }

  .trip-cards > p {
    margin-top: 0;
  }
</style>
