<script lang="ts">
import { goto } from "$app/navigation";
import { onMount } from "svelte";
import type { Trip } from "$types";
import PaymentTimelinePill from "./payment-timeline-pill.svelte";
import IdeaCard from "./idea-card.svelte";
import RecentTransactions from "./recent-transactions.svelte";
import BillPayment from "./bills/bill-payment.svelte";
import Trips from "$lib/trips.svelte";
export let title: string = "";
export let items: any[] = [];
export let trips: Trip[];
export let stats: any = {};
export let transactions: any[] = [];
export let billingStatements: any;
export let onRecordingPayment: any;
export let onTransactionSearch: any;
export let onCurrentBillStatementDoesNotExist: any;

let lastDay = 31;
let fullPaymentDates: any[] = [];
let month = "";

$: filteredItems = items.toSorted((p: any, q: any) => {
  if (!p.isEnabled) return Number.POSITIVE_INFINITY;
  if (!q.isEnabled) return Number.NEGATIVE_INFINITY;

  if (
    p.billingDate == null &&
    p.payByDate == null &&
    q.billingDate == null &&
    q.payByDate == null
  )
    return -1;

  return p.name.localeCompare(q.name);
});

onMount(() => {
  let lastDateOfMonth = new Date(
    new Date().getUTCFullYear(),
    new Date().getUTCMonth() + 1,
    0,
  );
  lastDay = lastDateOfMonth.getDate();
  month = Intl.DateTimeFormat(undefined, { month: "long" }).format(
    lastDateOfMonth,
  );
  if (title === "") title = `Timeline view for ${month}`;

  fullPaymentDates = Array.isArray(stats?.stats)
    ? stats?.stats?.filter((p: any) => p.type === "FULL_PAYMENT_DATES")
    : [];
});
</script>

<!-- <div class="timeline"> -->
  <!-- <div class="timeline-data"> -->
<section>
<div class="timeline-view">
    <RecentTransactions
      {transactions}
      showGraph={true}
      {title}
      groupTransactionByDate={false}
      showRecentSpends={false}
      showTotalSpend={false}
      initialShowCount={0}
    />

  </div>

<div class="timeline-view">
    <RecentTransactions
      {transactions}
      showGraph={false}
      title={"Your Recent Spends"}
      groupTransactionByDate={false}
      showRecentSpends={true}
      showTotalSpend={false}
      initialShowCount={5}
    />



    <Trips {trips} />
  </div>
<div class="bills-view">
    {#if filteredItems.length > 0}
      <h1 class="title_bill">Your bills</h1>
    {/if}

    <p class='stay-updated'>Stay updated with the bills you need to pay this month...</p>

    {#if fullPaymentDates.length > 0}
      <IdeaCard
        idea={`Pay all bills together by paying between ${month} ${fullPaymentDates[0].dateRanges[0].start} and ${month} ${fullPaymentDates[0].dateRanges[0].end}!`}
      />
    {/if}

    <div class="items bill-payments">
      {#each filteredItems.filter((p) => p.payByDate !== null && p.isEnabled) as item}
        <BillPayment
          bill={item}
          billingStatements={billingStatements?.[
            `billStatements__bill_${item.id}`
          ]}
          {onTransactionSearch}
          {onRecordingPayment}
          {onCurrentBillStatementDoesNotExist}
        />
      {/each}
    </div>

  </div>


<div class='billing-cycles-view'>
    <h1 class="title_bill">Billing Cycles</h1>

    <p>
      Each month, your bills are generated at the start of the bar, and are to
      be paid when the bar ends. Bars wrapping around mean they can be paid the
      next month
    </p>

    <div class="legend legend-top">
      <span>1</span>
      <span>{lastDay}</span>
    </div>

    <div class="items">
      {#each filteredItems as item}
        <button class="bill" on:click={() => goto(`bills?id=${item.id}`)}>
          <PaymentTimelinePill {item} />
        </button>
      {/each}
    </div>
    <div class="legend legend-bottom">
      <span>1</span>
      <span>{lastDay}</span>
    </div>

<div>
    <button
      class="cta"
      on:click={() => {
        goto("bills/add");
      }}
      >Add bill
    </button>
</div>
</div>
</section>

<style>
  section {
    display: grid;
    grid-template-columns: 100%;
    gap: 0;
    padding: 0;
  }

  .timeline-view,
  .bills-view,
  .billing-cycles-view {
    padding: 1rem;
    border-bottom: 1px solid var(--color-base-300);
  }

  .billing-cycles-view {
    border-bottom: none;
  }

  .title_bill {
    font-family: "Syne", system-ui, sans-serif;
    font-size: 0.6875rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--color-neutral-content);
    margin: 0 0 0.75rem 0;
  }

  .timeline-view {
    display: flex;
    flex-direction: column;
  }

  .stay-updated {
    font-size: 0.75rem;
    color: var(--color-neutral-content);
    margin-bottom: 0.75rem;
    margin-top: 0;
  }

  .bill {
    all: unset;
    width: 100%;
    padding: 0;
    cursor: pointer;
    display: block;
    border-radius: 0.375rem;
    transition: background-color 0.1s ease;
  }

  .bill:hover {
    background-color: rgba(28, 28, 38, 0.5);
  }

  :global(.bill-payments > div:first-of-type) {
    margin-top: 0;
  }

  .items {
    overflow-y: scroll;
    display: flex;
    flex-direction: column;
  }

  .items::-webkit-scrollbar {
    display: none;
  }

  .items {
    -ms-overflow-style: none;
    scrollbar-width: none;
  }

  .legend {
    display: flex;
    justify-content: space-between;
    font-family: "JetBrains Mono", monospace;
    font-size: 0.6875rem;
    font-weight: 500;
    margin: 0.25rem 0.25rem;
    color: #3a3a50;
  }

  .cta {
    display: block;
    width: 100%;
    margin-top: 1rem;
    padding: 0.625rem 1rem;
    background-color: rgba(0, 212, 184, 0.1);
    border: 1px solid rgba(0, 212, 184, 0.3);
    color: var(--color-primary);
    border-radius: 0.5rem;
    font-family: "Syne", system-ui, sans-serif;
    font-size: 0.8125rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    text-align: center;
    cursor: pointer;
    transition: all 0.2s ease;
  }

  .cta:hover {
    background-color: rgba(0, 212, 184, 0.18);
    border-color: rgba(0, 212, 184, 0.5);
  }

  p {
    font-size: 0.75rem;
    color: var(--color-neutral-content);
    margin: 0 0 0.75rem 0;
    line-height: 1.6;
  }

  @media (min-width: 65rem) {
    section {
      grid-template-columns: 1fr 1fr;
    }

    .timeline-view,
    .bills-view,
    .billing-cycles-view {
      border-right: 1px solid var(--color-base-300);
      border-bottom: none;
    }

    .billing-cycles-view,
    .bills-view {
      border-right: none;
    }
  }

  @media (min-width: 101rem) {
    section {
      grid-template-columns: repeat(4, 1fr);
    }
  }
</style>
