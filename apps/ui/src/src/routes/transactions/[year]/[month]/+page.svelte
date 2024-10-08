<script lang="ts">
  import { goto } from "$app/navigation";
  import RecentTransactions from "$lib/recent-transactions.svelte";
  import { paymentsUrql } from "$lib/stores/urql";
  import {
    faChevronLeft,
    faChevronRight,
  } from "@fortawesome/free-solid-svg-icons";

  import { queryStore, gql } from "@urql/svelte";
  import { onMount } from "svelte";
  import { Icon } from "svelte-awesome";

  let currentYear = new Date().getUTCFullYear();
  let currentMonth = new Date().getUTCMonth();

  onMount(() => {
    let path = window.location.pathname;
    currentYear = +path.split("/")[2];
    currentMonth = +path.split("/")[3];
  });

  $: transactionsQuery = queryStore({
    client: $paymentsUrql,
    variables: { year: currentYear, month: currentMonth + 1 },
    query: gql`
      query ($year: Int!, $month: Int!) {
        transactionsByYearAndMonth(year: $year, month: $month) {
          nodes {
            id
            amount
            merchant
            backDate
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

{#if $transactionsQuery.fetching}
  <p>Loading...</p>
{:else if $transactionsQuery.error}
  <p>🙆‍♂️ Uh oh! Unable to fetch your bills!</p>
{:else}
  <section class="title">
    <button
      on:click={() => {
        currentYear = currentMonth == 1 ? currentYear - 1 : currentYear;
        currentMonth = currentMonth == 1 ? 12 : currentMonth - 1;
        goto(`/transactions/${currentYear}/${currentMonth}`);
      }}
    >
      <Icon
        data={faChevronLeft}
        scale={1.5}
        style={`border-radius: 4rem; color: #5B81BB; padding: 0 1rem 0 .5rem; cursor: pointer; align-self: center`}
      />
    </button>
    <h1>
      {`Transactions for ${Intl.DateTimeFormat(undefined, {
        month: "short",
      }).format(new Date(currentYear, currentMonth, 1))} '${Intl.DateTimeFormat(
        undefined,
        {
          year: "2-digit",
        }
      ).format(new Date(currentYear, currentMonth, 1))}`}
    </h1>
    <button
      on:click={() => {
        currentYear = currentMonth == 12 ? currentYear + 1 : currentYear;
        currentMonth = currentMonth == 12 ? 1 : currentMonth + 1;
        goto(`/transactions/${currentYear}/${currentMonth}`);
      }}
    >
      <Icon
        data={faChevronRight}
        scale={1.5}
        style={`border-radius: 4rem; color: #5B81BB; padding: 0 1rem 0 .5rem; cursor: pointer; align-self: center`}
      />
    </button>
  </section>

  <RecentTransactions
    transactions={$transactionsQuery.data.transactionsByYearAndMonth.nodes}
    showAllTransactions={true}
    showViewAllCTA={false}
    title=""
  />
{/if}

<style>
  .title {
    display: flex;
    background-color: var(--primary-bg-color);
    padding-top: 1rem;
  }

  .title > h1 {
    flex-grow: 1;
    text-align: center;
    align-self: center;
  }

  .title button {
    background-color: unset;
    padding: 0.5rem;
  }
</style>
