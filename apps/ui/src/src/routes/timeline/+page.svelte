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
          isEnabled
        }
      }
    `,
  });

  const currentMonth = new Date().getUTCMonth() + 1
  const currentYear = new Date().getUTCFullYear()

  const billStatsQuery = queryStore({
    client: $urql,
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
</script>

<Nav />
{#if $billsQuery.fetching || $billStatsQuery.fetching}
  <p>Loading...</p>
{:else if $billsQuery.error || $billStatsQuery.error}
  <p>🙆‍♂️ Uh oh! Unable to fetch your bills!</p>
{:else}
  <Timeline items={$billsQuery.data.bills} stats={$billStatsQuery.data.billStats} />
{/if}

