<script lang="ts">
  import Icon from "svelte-awesome/components/Icon.svelte";
  import { faBars } from "@fortawesome/free-solid-svg-icons";
  import Timeline from "$lib/timeline.svelte";

  /** @type {import('./$types').PageData} */
  export let data: any;

  let vis;
</script>

<nav>
  <Icon
    data={faBars}
    scale={1.5}
    style="color: white; padding: 0.5rem 1rem; cursor: pointer;"
  />
  <h1>payobills</h1>
</nav>
<main>
  <h2>timeline view</h2>
  <Timeline items={data.bills} />
</main>

<style>
  h1 {
    font-size: 1.5rem;
    color: white;
    padding: 0.75rem 1rem 0.75rem 0;
    font-weight: 400;
    margin: 0;
  }

  h2 {
    font-weight: 400;
    color: #9f9f9f;
    font-size: 1rem;
  }

  nav {
    background-color: #5b81bb;
    display: flex;
    align-items: center;
    margin: 0;
    padding: 0;
  }

  main {
    margin: 1rem;
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }
</style>
