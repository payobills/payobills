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
  import { formatRelativeDate } from "../../../utils/format-relative-date";

  let transactionID: string | null = null;

  onMount(() => {
    let path = window.location.pathname;
    transactionID = path.split("/")[2];
  });

  $: transactionsQuery = queryStore({
    client: $paymentsUrql,
    variables: { id: transactionID },
    query: gql`
      query ($id: String!) {
        transactionByID(id: $id) {
          id
          amount
          merchant
          backDate
          transactionText
          tags
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
  <div class="transaction">
    <section class="title">
      <div class="amount">
        {#if $transactionsQuery.data.transactionByID.amount !== null}
          <span class="value"
            >{new Intl.NumberFormat(undefined, {
              style: "currency",
              currency: "INR",
            }).format($transactionsQuery.data.transactionByID.amount)}</span
          >
        {:else}
          <span>Unknown amount</span>
        {/if}
      </div>
      <div class="transaction-detail">
        {formatRelativeDate(
          new Date($transactionsQuery.data.transactionByID.backDate)
        )} • {new Intl.DateTimeFormat("en-GB", {
          year: "2-digit",
          month: "long",
          day: "2-digit",
          hour: "2-digit",
          minute: "2-digit",
          second: "2-digit",
        }).format(new Date($transactionsQuery.data.transactionByID.backDate))}
      </div>
    </section>
    <section class="content">
      {#if $transactionsQuery.data.transactionByID.merchant !== null}
        <h1>Merchant</h1>
        <h2>{$transactionsQuery.data.transactionByID.merchant}</h2>
      {:else}
        <h1>Unknown Merchant</h1>
      {/if}

      {#if $transactionsQuery.data.transactionByID.transactionText !== ""}
        <h1 class="subheader">Original Transaction Detail</h1>
        <div class="transaction-detail">
          {$transactionsQuery.data.transactionByID.transactionText}
        </div>
      {/if}

      <h1 class="subheader">Tags</h1>
      <div class="tags">
        {#each $transactionsQuery.data.transactionByID.tags as tag}
          <div class="tag">{tag}</div>
        {/each}
      </div>

    </section>
  </div>
{/if}

<style>
  .transaction {
    margin: 1rem;
  }

  .transaction-detail {
    font-size: 1rem;
    font-weight: 400;
    margin: 1rem auto;
  }

  p {
    margin: 0.75rem 1rem;
  }

  .title {
    background-color: var(--primary-bg-color);
    margin-bottom: 2rem;
  }

  h1 {
    flex-grow: 1;
    margin: 0;
  }

  .amount {
    margin: 1rem 0;
  }

  .amount > .value {
    font-size: 2rem;
  }

  .tags {
    display: flex;
    margin-top: 1rem;
    row-gap: 0.5rem;
    column-gap: 0.5rem;
    flex-wrap: wrap;
  }

  .tag {
    background-color: gray;
    border-radius: 5px;
    color: white;
    padding: 0.25rem;
  }
</style>
