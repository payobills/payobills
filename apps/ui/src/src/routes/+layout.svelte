<script>
  import { goto } from "$app/navigation";
  import BottomNav from "$lib/bottom-nav.svelte";
  import IconButton from "$lib/icon-button.svelte";
  import Nav from "$lib/nav.svelte";
  import { auth, loadAuthFromLocalStorage } from "$lib/stores/auth";
  import { tryLoadEnvUrls } from "$lib/stores/env";
  import { uiDrawer } from "$lib/stores/ui-drawer";
  import {
    faArrowAltCircleDown,
    faChevronCircleDown,
    faChevronDown,
  } from "@fortawesome/free-solid-svg-icons";
  import { onMount } from "svelte";
  import { get } from "svelte/store";
  import { fade, fly } from "svelte/transition";

  onMount(async () => {
    // Try to load env Urls from localStorage
    // await tryLoadEnvUrls();
    // Guard against going to other pages without login
    // loadAuthFromLocalStorage();
    // const authState = get(auth);
    // if (authState == null) await goto("/timeline");
    // setInterval(()=> {uiDrawer.update((curr) => ({...curr, state:!curr.state}))}, 2000)
  });

  function randomNotification() {
    Notification.requestPermission().then((result) => {
      if (result === "granted") {
        const notifTitle = "Payobills";
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

  const closeUiDrawer = () => {
    uiDrawer.update((curr) => {
      curr.onClose?.();
      return { ...curr, content: null };
    });
  };
</script>

<Nav />

<!-- <button on:click={randomNotification}>Notification</button> -->
<main>
  <slot />
  {#if $uiDrawer.content}
    <button
      class="drawer-transparent"
      aria-label="Transparent button to close bottom drawer"
      on:click={closeUiDrawer}
      in:fade={{ duration: 300 }}
      out:fade={{ duration: 300 }}
    ></button>
    <div
      class="drawer-container"
      in:fly={{ y: 800, duration: 300 }}
      out:fly={{ y: 800, duration: 300 }}
    >
      <div class="drawer-content-container">
        <button
          aria-label="drawer close button"
          class="drawer-close"
          on:click={closeUiDrawer}
        >
          <IconButton
            icon={faChevronDown}
            color="grey"
            backgroundColor={"transparent"}
            scale={0.75}
          />
        </button>
        <div class="drawer-content">
          {#if $uiDrawer.content}
            <svelte:component this={$uiDrawer.content} />
          {/if}
        </div>
      </div>
    </div>
  {/if}
</main>

<style>
  @import url("https://fonts.googleapis.com/css2?family=Montserrat:ital,wght@0,100..900;1,100..900&display=swap");
  @import url("https://fonts.googleapis.com/css2?family=Figtree:ital,wght@0,300..900;1,300..900&family=Vibes&display=swap");
  @import url("https://fonts.googleapis.com/css2?family=Vibes&display=swap");

  :root {
    --color: #9f9f9f;

    /* --primary-color: #181818;
    --primary-bg-color: #f3f3f3;
    --secondary-bg-color: #bbbbbb;
    --primary-accent-color: #3367d6; */

    --primary-color: #a0a0a8;
    --primary-bg-color: #181818;
    --secondary-bg-color: #bbbbbb;
    --primary-accent-color: #696adb;
    
  }

  .drawer-transparent {
    background-color: rgba(0, 0, 0, 0.75);
    position: absolute;
    top: 0;
    width: 100%;
    height: 100%;
    z-index: 1001;
  }

  .drawer-content-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    height: 100%;
  }

  .drawer-container {
    position: absolute;
    bottom: 0;
    background-color: var(--primary-bg-color);
    box-shadow: 0px 4px 16px black;
    width: calc(100% - 2rem);
    height: 70%;
    padding: 1rem;
    border-radius: 1rem 1rem 0 0;
    z-index: 1002;
  }

  .drawer-close {
    background-color: transparent;
    padding: 0;
  }

  .drawer-content {
    display: flex;
    flex-direction: column;
    width: 100%;
    height: 100%;
  }

  :global(a) {
    font-size: 0.8rem;
    color: var(--primary-color);
    font-weight: 400;
  }

  :global(p) {
    font-size: 0.75rem;
  }

  :global(div#app) {
    height: 100dvh;
  }

  :global(body, html) {
    margin: 0;
    padding: 0;
  }

  :global(body) {
    background-color: var(--primary-bg-color);
  }

  :global(#app) {
    display: flex;
    flex-direction: column;
  }

  :global(*) {
    color: #a0a0a8;
    font-family:
      "Figtree",
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
    background: var(--primary-accent-color);
    color: white;
    text-transform: uppercase;
    padding: 1rem;
  }

  :global(h1) {
    margin: 0.5rem 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--primary-color);
  }

  :global(h2) {
    font-size: 1rem;
    font-weight: 600;
  }

  main {
    margin: 0;
    display: flex;
    flex-direction: column;
    overflow-y: scroll;
    height: calc(100% - 4rem);
    align-self: stretch;
  }

  :global(h1, h2, h3, h4, h5, h6) {
    font-family:
      "Figtree",
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
  }
</style>
