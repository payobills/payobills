<script lang="ts">
export let item: any;
export let showLabel = true;
</script>

<div class="item">
  <div class="pill">
    {#if item.billingDate > item.payByDate}
      <span
        data-start={0}
        style={`flex-grow: ${item.payByDate}`}
        class="pill--filled"
      ></span>
      <span
        data-start={item.payByDate}
        style={`flex-grow: ${item.billingDate - item.payByDate + 1}`}
        class="pill--empty"
      ></span>
      <span
        data-start={item.billingDate}
        style={`flex-grow: ${31 - item.billingDate + 1}`}
        class="pill--filled"
      ></span>
    {:else}
      <span
        data-start={0}
        style={`flex-grow: ${item.billingDate}`}
        class="pill--empty"
      ></span>
      <span
        data-start={item.billingDate}
        style={`flex-grow: ${item.payByDate}`}
        class="pill--filled"
      ></span>
      <span
        data-start={item.payByDate}
        style={`flex-grow: ${31 - item.billingDate + 1}`}
        class="pill--empty"
      ></span>
    {/if}
  </div>
  {#if showLabel}
    <span data-start={0} data-length={item.payByDate} class="item-title"
      >{item.name}</span
    >
  {/if}
</div>

<style>
  .pill {
    display: flex;
    width: 100%;
    padding: 0.25rem 0;
    margin: 0;
    align-items: center;
  }
  .pill > span {
    height: 3px;
    border-radius: 2px;
    margin: 1rem 0 0 0;
  }
  .pill--filled {
    background-color: var(--color-primary);
    box-shadow: 0 0 4px rgba(0, 212, 184, 0.4);
  }
  .pill--empty {
    background-color: #1c1c26;
  }
  .item-title {
    font-family: "DM Sans", sans-serif;
    font-size: 0.6875rem;
    font-weight: 500;
    margin: 1.75rem 0;
    color: var(--color-neutral-content);
    letter-spacing: 0.01em;
  }
</style>
