<script>
  import Icon from "svelte-awesome/components/Icon.svelte";
  import { faBars } from "@fortawesome/free-solid-svg-icons";
</script>

<nav>
  <button>
    <Icon
      data={faBars}
      scale={1.5}
      style="color: white; padding: 1rem; cursor: pointer;"
    />
  </button>
  <h1>payobills</h1>
</nav>

<style>
  nav {
    background-color: #5b81bb;
    display: flex;
    align-items: center;
    margin: 0;
    padding: 0;
    height: 4rem;
    margin: 1rem 1rem 0 1rem;
    border-radius: calc(4em / 2);
  }

  button {
    align-self: center;
    background-color: #7e9dcd;
    border: none;
    border-radius: 1.75rem;
    padding: 0;
    margin: 0 0 0 0.2rem;
    height: 3.5rem;
    width: 3.5rem;
  }

  h1 {
    font-size: 1.5rem;
    color: white;
    padding: 0.75rem 1.5rem 0.75rem 1rem;
    font-weight: 400;
    margin: 0;
  }
  
</style>
