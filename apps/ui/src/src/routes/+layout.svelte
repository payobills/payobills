<script>
  import { goto } from "$app/navigation";
  import BottomNav from "$lib/bottom-nav.svelte";
  import Nav from "$lib/nav.svelte";
  import { auth, loadAuthFromLocalStorage } from "$lib/stores/auth";
  import { onMount } from "svelte";
  import { get } from "svelte/store";

  onMount(async () => {
    await goto("/timeline");
    // Guard against going to other pages without login
    // loadAuthFromLocalStorage();
    // const authState = get(auth);
    // if (authState == null) await goto("/timeline");
  });

  function randomNotification () {
    Notification.requestPermission().then((result) => {
    if (result === "granted") {
      const notifTitle ='Payobills';
      const notifBody = `You have a new bill to pay...`;
      const notifImg = `https://http.cat/images/200.jpg`;
      const options = {
        body: notifBody,
        icon: notifImg,
      };
      new Notification(notifTitle, options);
    }
  });
  }
</script>

<Nav />
<!-- <button on:click={randomNotification}>Notification</button> -->
<main>

  <slot />
</main>

<style>
  @import url("https://fonts.googleapis.com/css2?family=Montserrat:ital,wght@0,100..900;1,100..900&display=swap");

  :root {
    --color: #9f9f9f;

    --primary-color: #5b81bb;
    --primary-bg-color: #f3f3f3;
  }

  :global(div#app) {
    height: 100dvh;
  }

  :global(body, html) {
    margin: 0;
    padding: 0;
  }

  :global(body) {
    background-color: #5b81bb;
  }

  :global(#app) {
     display: flex;
     flex-direction: column;
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
    overflow-y: scroll;
    height: calc(100% - 4rem);
    background-color: var(--primary-bg-color);
  }

  :global(h1) {
    font-size: 1.2rem;
    font-weight: 600;
    color: var(--primary-color);
  }
</style>
