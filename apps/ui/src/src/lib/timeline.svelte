<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import PaymentTimelinePill from "./payment-timeline-pill.svelte";
  import IdeaCard from "./idea-card.svelte";
  import RecentTransactions from "./recent-transactions.svelte";
  import BillPayment from "./bills/bill-payment.svelte";
  export let title: string = "";
  export let items: any[] = [];
  export let stats: any = {};
  export let transactions: any[] = [];

  let lastDay = 31;
  let fullPaymentDates: any[] = [];
  let month = "";

  $: itemsFilteredByName = items.toSorted((p: any, q: any) =>
    p.name > q.name ? 1 : -1
  );
  $: filteredItems = itemsFilteredByName.toSorted((p: any, q: any) => {
    if (
      p.billingDate !== null &&
      p.payByDate !== null &&
      q.billingDate !== null &&
      q.payByDate !== null
    )
      return -1;
    return 0;
  });

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
    if (title === "") title = `Timeline view for ${month}`;

    fullPaymentDates = stats.stats.filter(
      (p: any) => p.type === "FULL_PAYMENT_DATES"
    );
  });
</script>

<div class="timeline">
  <div class="timeline-data">
    <RecentTransactions {transactions} showGraph={true} {title} />

    {#if filteredItems.length > 0}
      <h1 class="title_bill">Your bills</h1>
    {/if}

    {#if fullPaymentDates.length > 0}
      <IdeaCard
        idea={`Pay all bills together by paying between ${month} ${fullPaymentDates[0].dateRanges[0].start} and ${month} ${fullPaymentDates[0].dateRanges[0].end}!`}
      />
    {/if}

    <p>Your bills need attention this month...</p>
    <div class="items">
      {#each filteredItems.filter((p) => p.payByDate !== null) as item}
        <BillPayment bill={item} />
      {/each}
    </div>

    <div class="legend legend-top">
      <span>1</span>
      <span>{lastDay}</span>
    </div>

    <div class="items">
      {#each filteredItems as item}
        <button class="bill" on:click={() => goto(`bills/${item.id}`)}>
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
  .timeline {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    flex-grow: 1;
    background-color: #f3f3f3;
    padding-top: 0;
  }

  .title_bill {
    margin-top: 1rem;
  }
  .bill {
    all: unset;
    width: calc(100% - 1rem);
    margin: 0.5rem 0;
    padding: 0 0.5rem;
    align-self: flex-end;
    border-radius: 0.25rem;
  }
  .cta {
    margin: 1rem 0 0 0;
    border-radius: 1rem;
  }

  .timeline-data {
    display: flex;
    flex-direction: column;
    overflow-y: scroll;
  }

  .items {
    overflow-y: scroll;
    display: flex;
    flex-direction: column;
  }

  /* Hide scrollbar for Chrome, Safari and Opera */
  .items::-webkit-scrollbar {
    display: none;
  }

  /* Hide scrollbar for IE, Edge and Firefox */
  .items {
    -ms-overflow-style: none; /* IE and Edge */
    scrollbar-width: none; /* Firefox */
  }

  .legend {
    display: flex;
    justify-content: space-between;
    font-size: 0.75rem;
    margin: 0.5rem;
  }

  span {
    color: #9f9f9f;
  }
</style>
