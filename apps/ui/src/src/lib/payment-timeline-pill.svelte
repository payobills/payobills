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
    padding: 0.25rem 0 0.25rem 0;
    margin: 0;
  }
  .pill > span {
    height: 0.3125rem;
    border-radius: 0.25rem;
    margin: 1.25rem 0 0 0;
  }
  .pill--filled {
    background-color: var(--primary-color);
  }
  .pill--empty {
    background-color: #e1e1e1;
  }
  .item-title {
    font-size: 0.75rem;
    margin: 2rem 0;
    color: var(--color-primary]);
  }
</style>
