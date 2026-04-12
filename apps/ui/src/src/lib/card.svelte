<script lang="ts">
export let title: string = "";
</script>

<div class="card">
  {#if title !== ""}
    <div class="title">{title}</div>
  {/if}
  <slot />
</div>

<style>
  .card {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
    background-color: var(--color-base-200, #111118);
    border: 1px solid var(--color-base-300, #1c1c26);
    padding: 1rem;
    border-radius: 0.5rem;
  }

  .title {
    font-family: "Syne", system-ui, sans-serif;
    font-size: 0.6875rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--color-neutral-content, #8892a4);
    margin: 0 0 0.75rem 0;
  }
</style>
