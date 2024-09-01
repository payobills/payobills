<script lang="ts">
  import Timeline from "$lib/timeline.svelte";
  import RecentTransactions from "$lib/recent-transactions.svelte";
  import { goto } from "$app/navigation";
  import Nav from "$lib/nav.svelte";
  import {billsUrql, paymentsUrql } from '$lib/stores/urql'

  import { queryStore, gql, getContextClient } from "@urql/svelte";

  const billsQuery = queryStore({
    client: $billsUrql,
    query: gql`
      query {
        bills {
          id
          name
          billingDate
          payByDate
          isEnabled
        }
      }
    `,
  });

  const currentMonth = new Date().getUTCMonth() + 1
  const currentYear = new Date().getUTCFullYear()

  const billStatsQuery = queryStore({
    client: $billsUrql,
    query: gql`
        {
        billStats(year: ${currentYear.toString()}, month: ${currentMonth.toString()}) {
            startDate,
            endDate,
            stats {
                type
                billIds
                dateRanges {
                    start
                    end
                }
            }
        }
    }
    `,
  });

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

{#if $billsQuery.fetching || $billStatsQuery.fetching || $transactionsQuery.fetching}
  <p>Loading...</p>
{:else if $billsQuery.error || $billStatsQuery.error || $transactionsQuery.error}
  <p>🙆‍♂️ Uh oh! Unable to fetch your bills!</p>
{:else}
  <RecentTransactions transactions={$transactionsQuery.data.transactions} />
  <Timeline items={$billsQuery.data.bills} stats={$billStatsQuery.data.billStats} />
{/if}

