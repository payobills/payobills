<script>

</script>

{#if $nav.isOpen}
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
  <button class="app-title" on:click={() => goto($nav.link)}>
      {$nav.title}
  </button>
</nav>
{/if}

<style>
  nav {
    background-color: var(--primary-accent-color);
    display: flex;
    align-items: center;
    padding: 0;
    margin: 0;
    height: 4rem;
  }

  .app-title {
    font-size: 2rem;
    font-weight: 900;
    padding: 0;
    margin: 0 1rem;
    font-family: "Figtree";
    text-transform: capitalize;
    background-color: transparent;
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
