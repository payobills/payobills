<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  export let title: string = "";
  export let items: any[] = [];
  let lastDay = 31;

  onMount(() => {
    let lastDateOfMonth = new Date(
      new Date().getUTCFullYear(),
      new Date().getUTCMonth() + 1,
      0
    );
    lastDay = lastDateOfMonth.getDate();
    let month = Intl.DateTimeFormat(undefined, { month: "long" }).format(
      lastDateOfMonth
    );
    if (title === "") title = `timeline view for ${month.toLocaleLowerCase()}`;
  });
</script>

<div class="timeline">
  <div class="title">{title}</div>
  <div class="legend legend-top">
    <span>1</span>
    <span>{lastDay}</span>
  </div>
  <div class="items">
    {#each items as item}
      <div class="item">
        <div class="pill">
          {#if item.billingDate > item.payByDate}
            <span
              data-start={0}
              style={`flex-grow: ${item.payByDate}`}
              class="pill--filled"
            />
            <span
              data-start={item.payByDate}
              style={`flex-grow: ${item.billingDate - item.payByDate + 1}`}
              class="pill--empty"
            />
            <span
              data-start={item.billingDate}
              style={`flex-grow: ${31 - item.billingDate + 1}`}
              class="pill--filled"
            />
          {:else}
            <span
              data-start={0}
              style={`flex-grow: ${item.billingDate}`}
              class="pill--empty"
            />
            <span
              data-start={item.billingDate}
              style={`flex-grow: ${item.payByDate}`}
              class="pill--filled"
            />
            <span
              data-start={item.payByDate}
              style={`flex-grow: ${31 - item.billingDate + 1}`}
              class="pill--empty"
            />
          {/if}
        </div>
        <span data-start={0} data-length={item.payByDate} class="item-title"
          >{item.name}</span
        >
      </div>
    {/each}
  </div>
  <div class="legend legend-bottom">
    <span>1</span>
    <span>{lastDay}</span>
  </div>
  <button
    on:click={() => {
      goto("bills/add");
    }}
    >Add bill
  </button>
</div>

<style>
  .timeline {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    flex-grow: 1;
    background-color: #f3f3f3;
    padding: 1rem 1rem;
    border-radius: 2rem;
  }

  .title {
    color: #9f9f9f;
    margin: 0.5rem 0 1rem 0;
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
  button {
    margin: 1rem 0 0 0;
    padding: 1rem;
    align-self: flex-end;
  }
  .pill {
    display: flex;
    width: 100%;
    padding: 1rem 0 0.25rem 0;
  }
  .pill > span {
    height: 0.3125rem;
    border-radius: 0.25rem;
    margin: 1.5rem 0 0 0;
  }
  .pill--filled {
    background-color: #7a98c5;
  }
  .pill--empty {
    background-color: #e5ecf8;
  }
  .item-title {
    font-size: 0.75rem;
  }
  span {
    color: #9f9f9f;
  }
</style>
