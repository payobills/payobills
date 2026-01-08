<script lang="ts">
  import type { BillDTO } from '$lib/types';
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
  a {
    font-size: 0.75rem;
    margin-bottom: 1rem;
  }
</style>
