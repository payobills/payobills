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
    <p>Unknown billing period</p>
    {:else}
    <p>{statement.startDate} - {statement.endDate}</p>
    {/if}
  {/each}
{/if}

<style>
  p {
    font-size: 0.75rem;
  }
</style>
