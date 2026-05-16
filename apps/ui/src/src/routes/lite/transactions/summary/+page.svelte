<script lang="ts">
import { goto } from "$app/navigation";
import { paymentsUrql } from "$lib/stores/urql";
import { nav } from "$lib/stores/nav";
import { queryStore, gql } from "@urql/svelte";
import { onMount } from "svelte";
import dayjs from "dayjs";

let selectedDays: number = 30;
const periodOptions = [7, 30, 60, 90];

$: fromDate = dayjs().subtract(selectedDays, "day").toISOString();

onMount(() => {
  nav.update((prev) => ({ ...prev, isOpen: true }));
});

$: parseStatsQuery = queryStore({
  client: $paymentsUrql,
  variables: { fromDate },
  query: gql`
    query ParseStats($fromDate: String) {
      transactionStats(filters: { fromDate: $fromDate, scope: PARSE }) { stat value }
    }
  `,
});

$: tagStatsQuery = queryStore({
  client: $paymentsUrql,
  variables: { fromDate },
  query: gql`
    query TagStats($fromDate: String) {
      transactionStats(filters: { fromDate: $fromDate, scope: TAGS }) { stat value }
    }
  `,
});

$: currencyStatsQuery = queryStore({
  client: $paymentsUrql,
  variables: { fromDate },
  query: gql`
    query CurrencyStats($fromDate: String) {
      transactionStats(filters: { fromDate: $fromDate, scope: CURRENCY }) { stat value }
    }
  `,
});

const statLabels: Record<string, string> = {
  total: "Total",
  completed: "Completed",
  notStarted: "Not Started",
  pending: "Pending",
  failed: "Failed",
  tagged: "Tagged",
  untagged: "Untagged",
};

const statColors: Record<string, string> = {
  total: "var(--color-base-content)",
  completed: "var(--color-success, #22c55e)",
  notStarted: "var(--color-neutral-content)",
  pending: "var(--color-warning, #f59e0b)",
  failed: "var(--color-error, #ef4444)",
  tagged: "var(--color-primary)",
  untagged: "var(--color-neutral-content)",
};

function formatLabel(stat: string): string {
  if (statLabels[stat]) return statLabels[stat];
  if (stat.startsWith("currency_")) return stat.slice("currency_".length).toUpperCase();
  return stat;
}

function tileColor(stat: string): string {
  return statColors[stat] ?? "var(--color-primary)";
}

function onTileClick(_stat: string) {
  goto("/lite/transactions");
}
</script>

<section class="summary-page">
  <div class="period-selector">
    {#each periodOptions as days}
      <button
        class="period-btn"
        class:active={selectedDays === days}
        on:click={() => (selectedDays = days)}
      >
        Last {days}d
      </button>
    {/each}
  </div>

  <!-- Parse Stats -->
  <div class="scope-section">
    <h2 class="scope-title">Parse Status</h2>
    {#if $parseStatsQuery.fetching}
      <p class="status-msg">Loading…</p>
    {:else if $parseStatsQuery.error}
      <p class="status-msg error">Failed to load</p>
    {:else}
      <div class="tiles">
        {#each $parseStatsQuery.data?.transactionStats ?? [] as { stat, value }}
          <button class="tile" on:click={() => onTileClick(stat)}>
            <span class="tile-value" style="color: {tileColor(stat)}">{value}</span>
            <span class="tile-label">{formatLabel(stat)}</span>
          </button>
        {/each}
      </div>
    {/if}
  </div>

  <!-- Tag Stats -->
  <div class="scope-section">
    <h2 class="scope-title">Tags</h2>
    {#if $tagStatsQuery.fetching}
      <p class="status-msg">Loading…</p>
    {:else if $tagStatsQuery.error}
      <p class="status-msg error">Failed to load</p>
    {:else}
      <div class="tiles">
        {#each $tagStatsQuery.data?.transactionStats ?? [] as { stat, value }}
          <button class="tile" on:click={() => onTileClick(stat)}>
            <span class="tile-value" style="color: {tileColor(stat)}">{value}</span>
            <span class="tile-label">{formatLabel(stat)}</span>
          </button>
        {/each}
      </div>
    {/if}
  </div>

  <!-- Currency Stats -->
  <div class="scope-section">
    <h2 class="scope-title">Currency</h2>
    {#if $currencyStatsQuery.fetching}
      <p class="status-msg">Loading…</p>
    {:else if $currencyStatsQuery.error}
      <p class="status-msg error">Failed to load</p>
    {:else}
      <div class="tiles">
        {#each $currencyStatsQuery.data?.transactionStats ?? [] as { stat, value }}
          <button class="tile" on:click={() => onTileClick(stat)}>
            <span class="tile-value" style="color: {tileColor(stat)}">{value}</span>
            <span class="tile-label">{formatLabel(stat)}</span>
          </button>
        {/each}
      </div>
    {/if}
  </div>
</section>

<style>
  .summary-page {
    padding: 1rem;
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
  }

  .period-selector {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
  }

  .period-btn {
    padding: 0.375rem 0.75rem;
    border: 1px solid var(--color-base-300);
    border-radius: 0.375rem;
    background: transparent;
    color: var(--color-neutral-content);
    font-family: "Syne", sans-serif;
    font-size: 0.75rem;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.15s ease;
  }

  .period-btn:hover {
    border-color: rgba(0, 212, 184, 0.4);
    color: var(--color-primary);
  }

  .period-btn.active {
    border-color: var(--color-primary);
    color: var(--color-primary);
    background-color: rgba(0, 212, 184, 0.08);
  }

  .scope-section {
    border: 1px solid var(--color-base-300);
    border-radius: 0.5rem;
    padding: 1rem 1.25rem;
  }

  .scope-title {
    font-family: "Syne", sans-serif;
    font-size: 0.6875rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: #4a5568;
    margin: 0 0 0.875rem;
  }

  .tiles {
    display: flex;
    flex-wrap: wrap;
    gap: 0.625rem;
  }

  .tile {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.25rem;
    background-color: rgba(28, 28, 38, 0.5);
    border: 1px solid var(--color-base-300);
    border-radius: 0.375rem;
    padding: 0.75rem 1.25rem;
    min-width: 5rem;
    cursor: pointer;
    transition: border-color 0.15s ease, background-color 0.15s ease;
  }

  .tile:hover {
    border-color: rgba(0, 212, 184, 0.35);
    background-color: rgba(28, 28, 38, 0.8);
  }

  .tile-value {
    font-family: "Syne", sans-serif;
    font-size: 2rem;
    font-weight: 800;
    line-height: 1;
  }

  .tile-label {
    font-size: 0.625rem;
    color: var(--color-neutral-content);
    text-transform: uppercase;
    letter-spacing: 0.07em;
    font-weight: 600;
  }

  .status-msg {
    font-size: 0.8125rem;
    color: var(--color-neutral-content);
    margin: 0;
  }

  .status-msg.error {
    color: var(--color-error, #ef4444);
  }
</style>
