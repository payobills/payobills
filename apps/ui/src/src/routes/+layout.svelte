<script lang="ts">
  import { goto } from "$app/navigation";
  import BottomNav from "$lib/bottom-nav.svelte";
  import IconButton from "$lib/icon-button.svelte";
  import Nav from "$lib/nav.svelte";
  import { auth, loadAuthFromLocalStorage } from "$lib/stores/auth";
  import { tryLoadEnv } from "$lib/stores/env";
  import { uiDrawer } from "$lib/stores/ui-drawer";
  import {
    faChevronDown,
    faDownLeftAndUpRightToCenter,
    faUpRightAndDownLeftFromCenter
  } from "@fortawesome/free-solid-svg-icons";
  import { onMount } from "svelte";
  import { fade, fly } from "svelte/transition";
  import { nav } from "$lib/stores/nav";
  import { CONSTANTS } from '../constants';

  onMount(async () => {
    nav.update(prev => ({
      ...prev,
      title: CONSTANTS.PAYOBILLS,
      link: '/'
    }));
    
    // Try to load env Urls from localStorage
    await tryLoadEnv();

    // Guard against going to other pages without login
    // loadAuthFromLocalStorage();
    // const authState = get(auth);
    // if (authState == null) await goto("/timeline");
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

  const toggleFullScreenUiDrawer = () => {
    uiDrawer.update((curr) => {
      return {...curr, isFullScreen: !curr.isFullScreen };
    })
  }
</script>

<Nav />

<!-- <button on:click={randomNotification}>Notification</button> -->
<main class="bg-base-100" style={`${$nav.isOpen ? "height: calc(100% - 4rem)" : "height: 100%"}`}>
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
      class={`drawer-container ${$uiDrawer.isFullScreen ? "drawer-container--fullscreen" : ""}`}
      in:fly={{ y: 800, duration: 300 }}
      out:fly={{ y: 800, duration: 300 }}
    >
      <div class="drawer-content-container">
        <div class="drawer-action-icons">
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

        <button 
          aria-label="drawer close button"
          class="drawer-close"
          on:click={toggleFullScreenUiDrawer}
        >
          <IconButton
            icon={$uiDrawer.isFullScreen ? faDownLeftAndUpRightToCenter : faUpRightAndDownLeftFromCenter}
            color="grey"
            backgroundColor={"transparent"}
            scale={0.75}
          />
        </button>
      </div>


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

  @import "tailwindcss";

  @plugin "daisyui" {
    themes: dark --default; 
    root: ":root";
    logs: true;
  }

  :root {
    --color: #9f9f9f;

    --primary-color: #181818;
    --primary-bg-color: #f3f3f3;
    --secondary-bg-color: #bbbbbb;
    --primary-accent-color: #3367d6;

    --color-primary: #3367d6;
  }

  .drawer-transparent {
    background-color: rgba(0, 0, 0, 0.45);
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

  .drawer-action-icons {
    width: 100%;
    display: flex;
    justify-content: space-between;
    flex-direction: row;
  }

  .drawer-action-icons>*:first-child {
    flex-grow: 1;
  }

  .drawer-container {
    background-color: rgba(0, 0, 0, 0.75);
    position: absolute;
    bottom: 0;
    box-shadow: 0px 4px 16px black;
    width: calc(100%);
    height: 70%;
    padding: 1rem;
    border-radius: 1rem 1rem 0 0;
    transition: height cubic-bezier(0.19, 1, 0.22, 1) .3s;
    z-index: 1002;
  }

  .drawer-container--fullscreen {
    height: 100%;
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
    color: var(--color-primary);
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

  /* :global(body) { */
    /* background-color: var(--primary-bg-color); */
  /* } */

  :global(#app) {
    display: flex;
    flex-direction: column;
  }

  :global(*) {
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
    background-color: var(--color-primary);
    text-transform: uppercase;
  }

  :global(h1) {
    margin: 0.5rem 0;
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--color-primary)
  }

  :global(h2) {
    font-size: 1rem;
    font-weight: 600;
  }

  main {
    display: flex;
    flex-direction: column;
    overflow-y: scroll;
    align-self: stretch;
    /* padding: 1rem; */
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

