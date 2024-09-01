<script lang="ts">
    import Timeline from "$lib/timeline.svelte";
    import RecentTransactions from "$lib/recent-transactions.svelte";
    import { goto } from "$app/navigation";
    import Nav from "$lib/nav.svelte";
    import { paymentsUrql } from '$lib/stores/urql'
  
    import { queryStore, gql, getContextClient } from "@urql/svelte";
  
  
    const currentMonth = new Date().getUTCMonth() + 1
    const currentYear = new Date().getUTCFullYear()
  
    const transactionsQuery = queryStore({
      client: $paymentsUrql,
      query: gql`
      {
          transactions
          {
              id
              amount
              merchant
              backDateString
          }
      }
      `,
    });
  </script>
  
  {#if $transactionsQuery.fetching}
    <p>Loading...</p>
  {:else if $transactionsQuery.error}
    <p>🙆‍♂️ Uh oh! Unable to fetch your bills!</p>
  {:else}
    <RecentTransactions transactions={$transactionsQuery.data.transactions}
    showAllTransactions={true}
    showViewAllCTA={false} />
  {/if}
  
  