<script lang="ts">
  import Timeline from "$lib/timeline.svelte";
  import { goto } from "$app/navigation";
  import Nav from "$lib/nav.svelte";
  import {urql } from '$lib/stores/urql'

  import { queryStore, gql, getContextClient } from "@urql/svelte";

  const billsQuery = queryStore({
    client: $urql,
    query: gql`
      query {
        bills {
          id
          name
          billingDate
          payByDate
        }
      }
    `,
  });
</script>

<Nav />
{#if $billsQuery.fetching}
  <p>Loading...</p>
{:else if $billsQuery.error}
  <p>🙆‍♂️ Uh oh! Unable to fetch your bills!</p>
{:else}
  <Timeline items={$billsQuery.data.bills} />
{/if}

