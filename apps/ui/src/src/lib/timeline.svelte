<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import PaymentTimelinePill from "./payment-timeline-pill.svelte";
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
  <h1>{title}</h1>
  <div class="legend legend-top">
    <span>1</span>
    <span>{lastDay}</span>
  </div>
  <div class="items">
    {#each items as item}
      <button class="bill" on:click={() => goto(`bills?id=${item.id}`)}>
        <PaymentTimelinePill {item} />
      </button>
    {/each}
  </div>
  <div class="legend legend-bottom">
    <span>1</span>
    <span>{lastDay}</span>
  </div>
  <button
    class="cta"
    on:click={() => {
      goto("bills/add");
    }}
    >Add bill
  </button>
</div>

<style>
  .bill {
    all: unset;
    width: 100%;
    margin: 1rem 0 0 0;
    align-self: flex-end;
  }
  .cta {
    margin: 1rem 0 0 0;
    border-radius: 1rem;
  }

  .timeline {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    flex-grow: 1;
    background-color: #f3f3f3;
    padding: 1rem 1rem;
    border-radius: 2rem;
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

  span {
    color: #9f9f9f;
  }
</style>
