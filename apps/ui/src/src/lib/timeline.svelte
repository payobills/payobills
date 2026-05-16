<script lang="ts">
import { goto } from "$app/navigation";
import { onMount } from "svelte";
import type { Trip } from "$types";
import PaymentTimelinePill from "./payment-timeline-pill.svelte";
import IdeaCard from "./idea-card.svelte";
import RecentTransactions from "./recent-transactions.svelte";
import BillPayment from "./bills/bill-payment.svelte";
import Trips from "$lib/trips.svelte";
import { getBillPaymentCycle } from "../utils/get-bill-payment-cycle";
import { queryStore, gql } from "@urql/svelte";
import { paymentsUrql } from "$lib/stores/urql";
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

const firstOfMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString();
const firstOfMonthLabel = Intl.DateTimeFormat(undefined, { month: "long", day: "numeric" }).format(
  new Date(new Date().getFullYear(), new Date().getMonth(), 1)
);

const parseStatsQuery = queryStore({
  client: $paymentsUrql,
  variables: { fromDate: firstOfMonth },
  query: gql`
    query ParseStats($fromDate: String) {
      transactionStats(filters: { fromDate: $fromDate, scope: PARSE }) { stat value }
    }
  `,
});

const statLabels: Record<string, string> = {
  total: "Total", completed: "Completed", notStarted: "Not Started",
  pending: "Pending", failed: "Failed",
};
const statColors: Record<string, string> = {
  total: "var(--color-base-content)",
  completed: "var(--color-success, #22c55e)",
  notStarted: "var(--color-neutral-content)",
  pending: "var(--color-warning, #f59e0b)",
  failed: "var(--color-error, #ef4444)",
};

const isBillPaid = (bill: any): boolean => {
  const stmts = billingStatements?.[`billStatements__bill_${bill.id}`];
  if (!stmts) return false;
  const cycle = getBillPaymentCycle(bill);
  const stmt = stmts.find((s: any) => s.startDate === cycle?.fromDate && s.endDate === cycle?.toDate);
  return !!stmt?.isFullyPaid;
};

$: filteredItems = (billingStatements, items.toSorted((p: any, q: any) => {
  if (!p.isEnabled && !q.isEnabled) return 0;
  if (!p.isEnabled) return 1;
  if (!q.isEnabled) return -1;

  const pPaid = isBillPaid(p);
  const qPaid = isBillPaid(q);
  if (pPaid && !qPaid) return 1;
  if (!pPaid && qPaid) return -1;

  const todaysDay = new Date().getDate();
  const daysP = p.payByDate != null ? p.payByDate - todaysDay : Number.POSITIVE_INFINITY;
  const daysQ = q.payByDate != null ? q.payByDate - todaysDay : Number.POSITIVE_INFINITY;
  return daysP - daysQ;
}));

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
      showViewAllCTA={false}
      showDisclaimer={false}
      initialShowCount={0}
      chartHeight="100%"
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

<div class="stats-view">
  <div class="stats-header">
    <div>
      <h1 class="title_bill">Transaction Stats</h1>
      <p class="stats-since">Since {firstOfMonthLabel}</p>
    </div>
    <button class="view-all-btn" on:click={() => goto("/lite/transactions/summary")}>View all →</button>
  </div>
  {#if $parseStatsQuery.fetching}
    <p>Loading…</p>
  {:else if $parseStatsQuery.error}
    <p>Failed to load stats</p>
  {:else}
    <div class="stat-tiles">
      {#each $parseStatsQuery.data?.transactionStats ?? [] as { stat, value }}
        <button
          class="stat-tile"
          on:click={() => goto("/lite/transactions/summary")}
        >
          <span class="stat-tile-value" style="color: {statColors[stat] ?? 'var(--color-primary)'}">{value}</span>
          <span class="stat-tile-label">{statLabels[stat] ?? stat}</span>
        </button>
      {/each}
    </div>
  {/if}
</div>
</section>

<style>
  section {
    display: grid;
    grid-template-columns: 100%;
    gap: 0;
    padding: 0;
  }

  :global(.timeline-view .idea-card) {
    margin: 1rem 0;
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
    align-self: stretch;
  }

  .timeline-view:first-of-type {
    height: 100%;
  }

  .timeline-view:first-of-type :global(.container) {
    flex: 1;
    height: 100%;
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

  .stats-view {
    padding: 1rem;
    border-bottom: 1px solid var(--color-base-300);
  }

  .stats-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 0.75rem;
  }

  .stats-header .title_bill {
    margin: 0 0 0.125rem;
  }

  .stats-since {
    font-size: 0.6875rem;
    color: var(--color-neutral-content);
    margin: 0;
  }

  .view-all-btn {
    background: transparent;
    border: none;
    color: var(--color-primary);
    font-family: "Syne", sans-serif;
    font-size: 0.75rem;
    font-weight: 700;
    cursor: pointer;
    padding: 0;
  }

  .stat-tiles {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(4.5rem, 1fr));
    gap: 0.5rem;
  }

  .stat-tile {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.2rem;
    background-color: rgba(28, 28, 38, 0.5);
    border: 1px solid var(--color-base-300);
    border-radius: 0.375rem;
    padding: 0.625rem 1rem;
    cursor: pointer;
    transition: border-color 0.15s ease;
  }

  .stat-tile:hover {
    border-color: rgba(0, 212, 184, 0.35);
  }

  .stat-tile-value {
    font-family: "Syne", sans-serif;
    font-size: 1.375rem;
    font-weight: 800;
    line-height: 1;
  }

  .stat-tile-label {
    font-size: 0.625rem;
    color: var(--color-neutral-content);
    text-transform: uppercase;
    letter-spacing: 0.06em;
    font-weight: 600;
  }

  p {
    font-size: 0.75rem;
    color: var(--color-neutral-content);
    margin: 0 0 0.75rem 0;
    line-height: 1.6;
  }

  @media (min-width: 72rem) {
    section {
      grid-template-columns: 1fr 1fr;
      max-width: 90rem;
      margin: 0 auto;
    }

    .timeline-view:first-of-type {
      grid-column: 1;
      grid-row: 1;
    }

    .bills-view {
      grid-column: 1;
      grid-row: 2;
    }

    .timeline-view:last-of-type {
      grid-column: 2;
      grid-row: 1;
    }

    .billing-cycles-view {
      grid-column: 2;
      grid-row: 2;
    }

    .timeline-view,
    .bills-view,
    .billing-cycles-view {
      border-right: none;
      border-bottom: 1px solid var(--color-base-300);
    }

    .timeline-view:first-of-type,
    .timeline-view:last-of-type {
      border-right: 1px solid var(--color-base-300);
    }

    .timeline-view:last-of-type {
      border-right: none;
    }

    .bills-view,
    .billing-cycles-view {
      border-bottom: none;
    }

    .bills-view {
      border-right: 1px solid var(--color-base-300);
    }
  }
</style>
