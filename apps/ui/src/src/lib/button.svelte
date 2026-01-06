<script lang="ts">
const {
	children,
	onclick = undefined,
	state = "DEFAULT",
}: {
	children: any;
	onclick?: (event: any) => Promise<any | undefined>;
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
