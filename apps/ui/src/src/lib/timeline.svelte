<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import PaymentTimelinePill from "./payment-timeline-pill.svelte";
  import IdeaCard from "./idea-card.svelte";
  export let title: string = "";
  export let items: any[] = [];
  export let stats: any = {};
  let lastDay = 31;
  let fullPaymentDates: any[] = [];
  let month = '';

  onMount(() => {
    let lastDateOfMonth = new Date(
      new Date().getUTCFullYear(),
      new Date().getUTCMonth() + 1,
      0
    );
    lastDay = lastDateOfMonth.getDate();
    month = Intl.DateTimeFormat(undefined, { month: "long" }).format(
      lastDateOfMonth
    );
    if (title === "") title = `timeline view for ${month.toLocaleLowerCase()}`;

    fullPaymentDates = stats.stats.filter((p: any) => p.type === "FULL_PAYMENT_DATES")
  });
</script>

<div class="timeline">
  <div class="timeline-data">
  <h1>{title}</h1>
  {#if fullPaymentDates.length > 0}
    <IdeaCard idea={`Pay all bills together by paying between ${month} ${fullPaymentDates[0].dateRanges[0].start} and ${month} ${fullPaymentDates[0].dateRanges[0].end}!`}/>
  {/if}
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

  h1 {
    padding-top: 1rem;
  }
  .bill {
    all: unset;
    width: calc(100% - 1rem);
    margin: .5rem 0;
    padding:0 .5rem;
    align-self: flex-end;
    background-color: #d3d9e1;
    border-radius: .25rem;
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
    /* margin-bottom: 1rem; */
    overflow-y: scroll;
  }

  .timeline-data {
    display: flex;
    flex-direction: column;
    overflow-y: scroll;
  }

  .items {
    overflow-y: scroll;
    /* flex-grow: 1; */
    display: flex;
    flex-direction: column;
  }

  .legend {
    display: flex;
    justify-content: space-between;
    font-size: 0.75rem;
    margin: .5rem;
  }

  span {
    color: #9f9f9f;
  }
</style>
