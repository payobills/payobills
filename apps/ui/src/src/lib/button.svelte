<script lang="ts">
import IconButton from "./icon-button.svelte";
import { faCircleNotch } from "@fortawesome/free-solid-svg-icons";

let {
  children,
  onclick = undefined,
  state = "DEFAULT",
}: {
  children: any;
  onclick?: (event: any) => Promise<any | void>;
  state: "DEFAULT" | "LOADING" | "SUCCESS" | "ERROR";
} = $props();
</script>

<button
  onclick={async (ev) => {
    await onclick?.(ev);
  }}
  class="button-container"
>
  {#if state === "LOADING"}
    <IconButton icon={faCircleNotch} scale={0.75} />
  {:else}
    {@render children?.()}
  {/if}
</button>

<style>
  button {
    width: 100%;
    padding: 0.75rem 1.5rem;
    background-color: var(--color-primary);
    color: var(--color-primary-content);
    border: none;
    border-radius: 0.5rem;
    font-family: "Syne", system-ui, sans-serif;
    font-size: 0.875rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    cursor: pointer;
    transition: opacity 0.15s ease;
    margin-top: 0.5rem;
  }

  button:hover {
    opacity: 0.85;
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
