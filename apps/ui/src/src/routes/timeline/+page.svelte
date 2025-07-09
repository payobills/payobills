<script lang="ts">
  import Timeline from "$lib/timeline.svelte";
  import RecentTransactions from "$lib/recent-transactions.svelte";
  import { goto } from "$app/navigation";
  import Nav from "$lib/nav.svelte";
  import { billsUrql, paymentsUrql } from "$lib/stores/urql";

  import { queryStore, gql, getContextClient } from "@urql/svelte";
	import { Drawer } from "vaul-svelte";

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

  $: billStatementsQuery = (!$billsQuery.fetching && !$billsQuery.error) ? queryStore({
    client: $billsUrql,
    query:gql(`query { ${$billsQuery.data.bills.reduce((acc: string, currentBill: any) => acc += `billStatements__bill_${currentBill.id}: billStatements(billId: "${currentBill.id}") {
      id
      startDate
      endDate
      amount
    }
     `, '')} }`) }) : null;

  const currentMonth = new Date().getUTCMonth() + 1;
  const currentYear = new Date().getUTCFullYear();

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

  $: transactionsQuery = queryStore({
    client: $paymentsUrql,
    variables: { year: currentYear, month: currentMonth },
    query: gql`
      query ($year: Int!, $month: Int!) {
        transactions: transactionsByYearAndMonth(year: $year, month: $month, first: 900) {
          nodes {
            id
            amount
            merchant
            paidAt
            tags
          }
          pageInfo {
            hasNextPage
            startCursor
            endCursor
          }
        }
      }
    `,
  });
</script>

<section class="timeline-page">
  {#if $billsQuery.fetching || $billStatsQuery.fetching || $transactionsQuery.fetching}
    <p>Loading...</p>
  {:else if $billsQuery.error || $billStatsQuery.error || $transactionsQuery.error}
    <p>🙆 Uh oh! Unable to fetch your bills!</p>
  {:else}
    <Timeline
      items={$billsQuery.data.bills}
      stats={$billStatsQuery.data.billStats}
      transactions={$transactionsQuery.data.transactions.nodes}
      billingStatements={$billStatementsQuery?.data}
    />
  {/if}
</section>

<style>
  .timeline-page {
    margin: 1rem;
  }

  p {
    margin: 0;
  }
</style>
