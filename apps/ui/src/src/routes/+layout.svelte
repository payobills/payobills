<script>
  import { goto } from "$app/navigation";
  import { auth, loadAuthFromLocalStorage } from "$lib/stores/auth";
  import { onMount } from "svelte";
  import { get } from "svelte/store";

  onMount(async () => {
    // Guard against going to other pages without login
    loadAuthFromLocalStorage()
    const authState = get(auth);
    if (authState == null) await goto("/");
  });
</script>

<main>
  <slot />
</main>

<style>
  @import url('https://fonts.googleapis.com/css2?family=Montserrat:ital,wght@0,100..900;1,100..900&display=swap');

  :root {
    --color: #9f9f9f;
  }

  :global(body, html) {
    margin: 0;
    padding: 0;
  }
  :global(*) {
    font-family:
      Montserrat,
      "Helvetica Neue",
      system-ui,
      -apple-system,
      BlinkMacSystemFont,
      "Segoe UI",
      Roboto,
      Oxygen,
      Ubuntu,
      Cantarell,
      "Open Sans",
      sans-serif;
    font-weight: 400;
  }

  :global(button) {
    border: none;
    border-radius: 0.25rem;
    background: #5b81bb;
    color: white;
    text-transform: uppercase;
    padding: 1rem;
  }

  :global(h1) {
    margin: 0.5rem 0;
    font-size: 1.5rem;
  }

  main {
    margin: 0;
    display: flex;
    flex-direction: column;
    flex-grow: 1;
    height: 100dvh;
  }

  /* :global { */
    :root {
      --primary-color: #5B81BB;
    }
  /* } */
</style>
