<script>
  import Icon from "svelte-awesome/components/Icon.svelte";
  import { faBars } from "@fortawesome/free-solid-svg-icons";
  import { goto } from "$app/navigation";
</script>

<nav>
  <button class="hamburger" role="button">
    <Icon
      data={faBars}
      scale={1.2}
      style="color: white; padding: 0; cursor: pointer;"
    />
  </button>
  <button class="app-title" on:click={() => goto("/timeline")}>Payobills</button
  >
</nav>

<style>
  nav {
    background-color: #5b81bb;
    display: flex;
    align-items: center;
    padding: 0;
    margin: 0;
    height: 4rem;
  }
  .app-title {
    font-size: 1.2rem;
    font-weight: 700;
    text-transform: unset;
    padding: 0;
  }

  .hamburger {
    align-self: center;
    border: none;
    border-radius: 1.75rem;
    padding: 0;
    margin: 0 0 0 0.2rem;
    width: 4rem;
    padding-top: .25rem;
  }
</style>
