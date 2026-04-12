<script lang="ts">
import type { BillDTO } from "$lib/types";
export let bill: BillDTO;
export let statements: any[];
</script>

<h2>Bill Statements</h2>
{#if statements?.length === 0}
  <p>No statements found this bill</p>
{:else}
  {#each statements as statement}
    {#if statement.startDate === null || statement.endDate === null}
    <a href={`/bills/bill-statements?bill-id=${bill.id}&bill-statement-id=${statement.id}`}>Unknown billing period</a>
    {:else}
    <a href={`/bills/bill-statements?bill-id=${bill.id}&bill-statement-id=${statement.id}`}>{statement.startDate} - {statement.endDate}</a>
    {/if}
  {/each}
{/if}

<style>
  h2 {
    font-family: "Syne", sans-serif;
    font-size: 0.6875rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--color-neutral-content, #8892a4);
    margin: 1rem 0 0.5rem 0;
  }

  a {
    display: block;
    font-family: "JetBrains Mono", monospace;
    font-size: 0.75rem;
    color: var(--color-primary, #00d4b8);
    margin-bottom: 0.375rem;
    padding: 0.375rem 0.5rem;
    background-color: rgba(0, 212, 184, 0.05);
    border: 1px solid rgba(0, 212, 184, 0.12);
    border-radius: 0.375rem;
    transition: all 0.15s ease;
  }

  a:hover {
    background-color: rgba(0, 212, 184, 0.1);
  }
</style>
