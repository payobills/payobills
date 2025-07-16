<script lang="ts">
  import IconButton from "./icon-button.svelte";
  import { faCircleNotch } from "@fortawesome/free-solid-svg-icons";

  let { children, onclick = () => {}, isLoading = true } = $props();
</script>

<button onclick={(ev) => onclick(ev)} class="button-container">
  {#if isLoading}
    <IconButton icon={faCircleNotch} scale={0.75} />
  {:else}
    {@render children?.()}
  {/if}
</button>

<style>
  button {
    width: 100%;
  }

  :global(.button-container > .icon-button) {
    animation: spin 1.5s linear infinite;
  }

  @keyframes spin {
    100% {
      transform: rotate(360deg);
    }
  }
</style>
