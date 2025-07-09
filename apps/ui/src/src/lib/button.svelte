<script lang="ts">
  import IconButton from "./icon-button.svelte";
  import {
    faCircleNotch,
  } from "@fortawesome/free-solid-svg-icons";

  let { children, onclick = () => {}, isLoading = false } = $props();
</script>

<button onclick={(ev) => onclick(ev)} class="button-container">
  {#if isLoading}
    <IconButton icon={faCircleNotch} />
  {:else}
    {@render children?.()}
  {/if}
</button>

<style>
  :global(.button-container > .icon-button) {
    animation: spin 1.5s linear infinite;
  }

  @keyframes spin {
    100% {
      transform: rotate(360deg);
    }
  }
</style>
