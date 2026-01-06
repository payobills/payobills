<script lang="ts">
  import type { Trip } from '$lib/types';
  import { paymentsUrql } from "$lib/stores/urql";
  import { queryStore, gql } from "@urql/svelte";

  $: transactionTagsQuery = queryStore({
    client: $paymentsUrql,
    query: gql`
      query {
        transactionTags {
          id
          title
        }
      }
    `,
  });

  $: trips = $transactionTagsQuery?.data?.transactionTags
      ?.filter((p: Trip) => p.title.startsWith("Trip"))
      .map((p: Trip) => ({ ...p, title: p.title.replace(/^Trip:?\w?/, '')})) ?? []


</script>

<section>
  <h1>Your Trips</h1>

  {#if !$transactionTagsQuery?.fetching && $transactionTagsQuery.error} 
<div>There was an issue getting your trips...</div>

    {:else}
    {#each trips as trip}
      <h2>{trip.title}</h2>
    {/each}
{/if}
</section>


<style>
section {
  padding: 1rem;
}
</style>
