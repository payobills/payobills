<script lang="ts">
export const title: string = "";
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
    /* background-color: #f3f3f3; */
    background-color: rgb(59, 59, 59);
    /* background-color: #f3f3f3; */
    padding: 1rem 1rem;
    border-radius: 0.425rem;
  }

  .title {
    color: #9f9f9f;
    margin: 0.5rem 0 1rem 0;
  }
</style>
