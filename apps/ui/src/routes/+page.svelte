<script lang='ts'>
  import Card from '$lib/card.svelte';

  /** @type {import('./$types').PageData} */
  export let data: any;
</script>

<nav>
  <h1>{data.appInfo.app}</h1>
</nav>
<main>
  {#each data.bills as bill}
  <Card title={bill.name}/>
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

  nav {
    background-color: #5b81bb;
    margin: 0;
    padding: 0;
  }
</style>
