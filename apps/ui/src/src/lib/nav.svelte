<script>
  import Icon from "svelte-awesome";
  import { faBars } from "@fortawesome/free-solid-svg-icons";
  import { goto } from "$app/navigation";
</script>

<nav>
  <!--
  <button class="hamburger" role="button">
    <Icon
      data={faBars}
      scale={1.2}
      style="color: white; padding: 0; cursor: pointer;"
    />
  </button>
  -->
  <button class="app-title" on:click={() => goto("/timeline")}>
    payobills
  </button>
</nav>

<style>
  nav {
    background-color: var(--primary-color);
    display: flex;
    align-items: center;
    padding: 0;
    margin: 0;
    height: 4rem;
  }

  .app-title {
    font-size: 2.25rem;
    font-weight: 100;
    padding: 0;
    margin: 0 1rem;
    font-family: "Vibes";
    text-transform: lowercase;
  }

  /* .hamburger {
    align-self: center;
    border: none;
    border-radius: 1.75rem;
    padding: 0;
    margin: 0 0 0 0.2rem;
    width: 3rem;
    height: 100%;
    padding-top: .25rem;
  } */
</style>
