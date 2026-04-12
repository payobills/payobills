<script lang="ts">
import { goto } from "$app/navigation";
import RecentTransactions from "$lib/recent-transactions.svelte";
import { paymentsUrql } from "$lib/stores/urql";
import { nav } from "$lib/stores/nav";
import {
  faChevronLeft,
  faChevronRight,
} from "@fortawesome/free-solid-svg-icons";

import { onMount } from "svelte";
import { Icon } from "svelte-awesome";
import { liteServices } from "../../../lib/stores/lite-services";

$: transactionsService = $liteServices?.transactionsService;
let transactionId: string;
let currentYear: number;

onMount(() => {
  nav.update((prev) => ({ ...prev, isOpen: true, title: CONSTANTS.PAYOBILLS }));
  transactionId = new URLSearchParams(window.location.search)?.get("id") ?? "";
});

$: transactionsQuery =
  currentYear && currentMonth
    ? transactionsService?.queryTransactions({ filters: { ids: [] } })
    : null;
</script>

<section class="monthly-transactions">
  {#if $transactionsQuery == null || $transactionsQuery.fetching}
    <p>Loading...</p>
  {:else if $transactionsQuery?.error}
    <p>🙆‍♂️ Uh oh! Unable to fetch your bills!</p>
  {:else}
    <section class="title">
      <button
        on:click={() => {
          currentYear = currentMonth == 1 ? currentYear - 1 : currentYear;
          currentMonth = currentMonth == 1 ? 12 : currentMonth - 1;
          goto(`transactions?year=${currentYear}&month=${currentMonth}`);
        }}
      >
        <Icon
          data={faChevronLeft}
          scale={1.5}
          style={`border-radius: 4rem; color: var(--primary-color); padding: 0 1rem 0 .5rem; cursor: pointer; align-self: center`}
        />
      </button>
      <h1>
        {`Transactions for ${Intl.DateTimeFormat(undefined, {
          month: "short",
        }).format(
          new Date(currentYear, currentMonth - 1, 1)
        )} '${Intl.DateTimeFormat(undefined, {
          year: "2-digit",
        }).format(new Date(currentYear, currentMonth-1, 1))}`}
      </h1>
      <button
        on:click={() => {
          currentYear = currentMonth == 12 ? currentYear + 1 : currentYear;
          currentMonth = currentMonth == 12 ? 1 : currentMonth + 1;
          goto(`transactions?year=${currentYear}&month=${currentMonth}`);
        }}
      >
        <Icon
          data={faChevronRight}
          scale={1.5}
          style={`border-radius: 4rem; color: var(--primary-color); padding: 0 1rem 0 .5rem; cursor: pointer; align-self: center`}
        />
      </button>
    </section>

    <RecentTransactions
      transactions={$transactionsQuery.data.transactionsByYearAndMonth.nodes.filter(
        (p) => !p.tags.includes("Payment")
      )}
      showAllTransactions={true}
      showGraph={true}
      showViewAllCTA={false}
      title=""
    />
  {/if}
</section>

<style>
  .monthly-transactions {
    padding: 1rem;
  }

  p {
    margin: 0;
    font-size: 0.8125rem;
    color: var(--color-neutral-content);
  }

  .title {
    display: flex;
    align-items: center;
    background-color: transparent;
    margin-bottom: 1rem;
    padding-bottom: 0.75rem;
    border-bottom: 1px solid var(--color-base-300);
  }

  .title > h1 {
    flex-grow: 1;
    text-align: center;
    align-self: center;
    font-family: "Syne", sans-serif;
    font-size: 0.9375rem;
    font-weight: 700;
    letter-spacing: -0.01em;
    margin: 0;
  }

  .title button {
    background-color: transparent;
    border: 1px solid var(--color-base-300);
    border-radius: 0.375rem;
    padding: 0.4rem 0.5rem;
    color: var(--color-neutral-content);
    transition: all 0.15s ease;
  }

  .title button:hover {
    border-color: rgba(0, 212, 184, 0.3);
    color: var(--color-primary);
  }
</style>

