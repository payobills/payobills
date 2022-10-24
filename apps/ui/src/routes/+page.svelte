<script lang="ts">
  import Card from "$lib/card.svelte";

  /** @type {import('./$types').PageData} */
  export let data: any;
</script>

<nav>
  <h1>payobills</h1>
</nav>
<main>
  <h2>your bills</h2>
  {#each data.bills as bill}
    <Card title={bill.name} />
  {/each}
</main>

<style>
  :global(body, html) {
    margin: 0;
    padding: 0;
  }
  :global(*) {
    font-family: Quicksand, "Work Sans", sans-serif;
  }

  h1 {
    font-size: 1.5rem;
    color: white;
    padding: 0.75rem 1rem;
    font-weight: 100;
    margin: 0;
  }

  h2 {
    font-weight: 100;
    font-size: 1rem;
  }

  nav {
    background-color: #5b81bb;
    margin: 0;
    padding: 0;
  }

  main {
    margin: 1rem;
  }
</style>
