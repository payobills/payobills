<script>
  import Icon from "svelte-awesome/components/Icon.svelte";
  import { faBars } from "@fortawesome/free-solid-svg-icons";
  import { goto } from "$app/navigation";
</script>

<nav>
  <button class="hamburger">
    <Icon
      data={faBars}
      scale={1.5}
      style="color: white; padding: 1rem; cursor: pointer;"
    />
  </button>
  <button class="app-title" on:click={() => goto("/timeline")}>payobills</button
  >
</nav>

<style>
  nav {
    background-color: #5b81bb;
    display: flex;
    align-items: center;
    padding: 0;
    height: 4rem;
    margin: 0 0 1rem 0;
  }
  .app-title {
    font-size: 1.5rem;
    text-transform: lowercase;
  }

  .hamburger {
    align-self: center;
    border: none;
    border-radius: 1.75rem;
    padding: 0;
    margin: 0 0 0 0.2rem;
    height: 3.5rem;
    width: 3.5rem;
  }
</style>
