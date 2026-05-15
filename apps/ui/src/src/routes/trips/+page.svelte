<script lang="ts">
import { gql, queryStore } from "@urql/svelte";
import { onMount } from "svelte";
import { paymentsUrql } from "$lib/stores/urql";
import type { Trip } from "$lib/types";
import RecentTransactions from "../../lib/recent-transactions.svelte";
import { formatCurrencyAmount } from "../../utils/currency-formatter.util";

let tripId: string | undefined;
let tripTitle: string | undefined;

onMount(() => {
  tripId = new URLSearchParams(window.location.search)?.get("id") ?? "";
});

$: tripsQuery = queryStore({
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

$: {
  tripTitle =
    tripId && $tripsQuery.data
      ? $tripsQuery.data.transactionTags.find((tag: Trip) => tag.id === tripId)
          ?.title
      : "";
}

$: transactionsForTripQuery =
  tripId && tripTitle
    ? queryStore({
        client: $paymentsUrql,
        query: gql`
      query TransactionsForTrip($tags: [String!]!) {
        transactions(filters: { tags: $tags }, first: 50) {
          nodes {
            id
            amount
            currency
            merchant
            createdAt
            paidAt
            updatedAt
            tags
          }
        }
      }
    `,
        variables: {
          tags: [tripTitle],
        },
      })
    : null;

$: spendByCurrency = ($transactionsForTripQuery?.data?.transactions?.nodes ?? [])
  .filter((t: any) => t.paidAt)
  .reduce((acc: Record<string, number>, t: any) => {
    const code = t.currency ?? "INR";
    acc[code] = (acc[code] ?? 0) + t.amount;
    return acc;
  }, {});
</script>

<section>
  <div class="content">
    <h1>{tripTitle}</h1>
  </div>

  {#if Object.keys(spendByCurrency).length > 0}
    <div class="spend-summary">
      <span class="spend-label">Total spend</span>
      <div class="spend-amounts">
        {#each Object.entries(spendByCurrency) as [currency, amount]}
          <span class="spend-amount">{formatCurrencyAmount(amount, currency)}</span>
        {/each}
      </div>
    </div>
  {/if}

  <div class="content">
    <RecentTransactions
      title=""
      viewType={'all'}
      showAllTransactions={true}
      showGraph={false}
      showViewAllCTA={false}
      showTotalSpend={false}
      transactions={$transactionsForTripQuery?.data?.transactions?.nodes ?? []}
    />
  </div>
</section>

<style>
  section {
    padding: 1rem 0;
  }

  .content {
    padding: 0 1rem;
  }

  .spend-summary {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    padding: 0.75rem 1rem;
    margin-bottom: 0.5rem;
    background-color: var(--color-base-200);
  }

  .spend-label {
    font-weight: 600;
  }

  .spend-amounts {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 0.25rem;
  }

  .spend-amount {
    font-weight: 600;
    font-size: 1.35rem;
  }
</style>
