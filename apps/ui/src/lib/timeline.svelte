<script lang="ts">
  export let items: any[] = [];
</script>

<div class="timeline">
  <div class="legend legend-top">
    <span>1</span>
    <span>31</span>
  </div>
  <div class="items">
    {#each items as item}
      <div class="item">
        <div class='pill'>
        {#if item.billingDate > item.payByDate}
        <span data-start={0} style={`flex-grow: ${item.payByDate}`} class='pill--filled'></span>
        <span data-start={item.payByDate} style={`flex-grow: ${item.billingDate - item.payByDate + 1}`} class='pill-empty'></span>
        <span data-start={item.billingDate} style={`flex-grow: ${31 - item.billingDate + 1}`} class='pill--filled'></span>
        {:else}
        <span data-start={0} style={`flex-grow: ${item.payByDate}`} class='pill-empty'></span>
        <span data-start={item.payByDate} style={`flex-grow: ${ item.payByDate -item.billingDate + 1}`} class='pill--filled'></span>
        <span data-start={item.billingDate} style={`flex-grow: ${31 - item.billingDate + 1}`} class='pill-empty'></span>
      {/if}  
      </div>
        <span data-start={0} data-length={item.payByDate} class='item-title'>{item.name}</span>
      </div>
    {/each}
  </div>
  <div class="legend legend-bottom">
    <span>1</span>
    <span>31</span>
  </div>
</div>

<style>
  .timeline {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    flex-grow: 1;
    background-color: #d9d9d9;
    padding: 1.5rem 1rem;
    border-radius: 0.5rem;
  }

  .items {
    flex-grow: 1;
    display: flex;
    flex-direction: column;
  }

  .legend {
    display: flex;
    justify-content: space-between;
    font-size: 0.75rem;
  }

  .pill {display: flex; width: 100%; padding: 1rem 0 0.25rem 0;}
  .pill > span {
    height: 0.25rem;
    border-radius: 0.25rem;
  }
  .pill--filled{background-color: #5b81bb;}
  .item-title {font-size: 0.75rem;}
  span {
    color: #9f9f9f;
  }
</style>
