<script lang="ts">
  export let bill: any;
  export let statements: any[];
</script>

<h2>bill statements</h2>
{#if statements?.length === 0}
  <p>No statements found this bill</p>
{:else}
  {#each statements as statement}
    {#if statement.startDate === null || statement.endDate === null}
    <a href={`/bills/${bill.id}/bill-statements/${statement.id}`}>Unknown billing period</a>
    {:else}
    <a href={`/bills/${bill.id}/bill-statements/${statement.id}`}>{statement.startDate} - {statement.endDate}</a>
    {/if}
  {/each}
{/if}

<style>
  a {
    font-size: 0.75rem;
    margin-bottom: 1rem;
  }
</style>
