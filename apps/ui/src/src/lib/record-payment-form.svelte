<script lang="ts">
import { writable } from "svelte/store";
import BillPayment from "./bills/bill-payment.svelte";
import Button from "./button.svelte";
import Card from "./card.svelte";
import type { BillStatementDTO, TransactionDTO } from "./types";
import { formatRelativeDate } from "../utils/format-relative-date";

export let bill: any;
export let billStatements: BillStatementDTO[];
export let onRecordingPayment;
export let lockBillStatementCycle = false;
export let selectedStatement: BillStatementDTO;

export let onTransactionSearch: any;
export let onLoadMoreTransactions: any;
export let initialTransactions: any[] = [];
let matchingTransactionsQuery = writable<any>({
  data: initialTransactions,
  fetching: false,
  error: null,
  endCursor: null as string | null,
  hasNextPage: true,
});

let loadingMore = false;
const loadMore = async () => {
  loadingMore = true;
  try {
    const after = $matchingTransactionsQuery.endCursor;
    const first = after === null ? $matchingTransactionsQuery.data.length + 5 : 5;
    await onLoadMoreTransactions(matchingTransactionsQuery, after, first);
  } finally {
    loadingMore = false;
  }
};

let transactionSearchTerm: string = "";
$: selectedTransactions = ($matchingTransactionsQuery?.data?.reduce(
  (acc, curr) => {
    acc = { ...acc, [curr.id.toString()]: new IsSelected(false, curr) };
    return acc;
  },
  {},
) ?? {}) satisfies Record<string, IsSelected<TransactionDTO>>;
$: amount = Object.values(selectedTransactions)
  .filter((x) => x.state)
  .reduce((acc, curr) => acc + curr.metadata?.amount || 0, 0);

let isFullyPaid = true;
let isSaving = false;

class IsSelected<T> {
  constructor(
    private __state: boolean,
    private __metadata: T,
  ) {}
  get state() {
    return this.__state;
  }
  set state(newState: boolean) {
    this.__state = newState;
  }
  get metadata(): T {
    return this.__metadata;
  }
}

const onAmountSearchBoxFocusout = () => {
  try {
    onTransactionSearch(matchingTransactionsQuery, transactionSearchTerm);
  } catch {}
};
</script>

<div class="container">
  <div class="form-container">
    <h2>Record a payment</h2>
    <label for="record-bill-payment-bill">Bill</label>
    <BillPayment
      {bill}
      billingStatements={[]}
      showRecordPaymentButton={false}
      onRecordingPayment={() => {}}
      currentCycleFromDate={selectedStatement.startDate}
      currentCycleToDate={selectedStatement.endDate}
      showBillingCycle={!lockBillStatementCycle}
    />

    <label for="record-bill-statement">Bill Cycle</label>
      {#each billStatements as statement (statement.id)}
        <div class={`bill-statement ${lockBillStatementCycle ? "bill-statement--locked" : ""} ${statement.id === selectedStatement.id ? "bill-statement--selected": ""}`}>
          <Card>
            <!-- <p>{`${statement.id}`}</p> -->
            <p>{`${statement.startDate} - ${statement.endDate}`}</p>
          </Card>
        </div>
      {/each}

    <label for="record-bill-payment-amount">Amount</label>
    <input
      id="record-bill-payment-amount"
      placeholder="Enter amount paid or search"
      bind:value={transactionSearchTerm}
      on:keydown={(e) =>{ if(e.key === 'Enter') onAmountSearchBoxFocusout() }}
    />

    <div class="txn-list">
      {#each $matchingTransactionsQuery?.data ?? [] as transaction (transaction.id)}
        <label class="txn-row" class:txn-row--selected={selectedTransactions[transaction.id]?.state}>
          <input
            type="checkbox"
            bind:checked={selectedTransactions[transaction.id].state}
            name={`record-bill-payment-transaction-${transaction.id}`}
          />
          <div class="txn-details">
            <span class="txn-merchant">{transaction.merchant ?? 'Unknown Merchant'}</span>
            <span class="txn-date">{formatRelativeDate(new Date(transaction.paidAt))}</span>
          </div>
          <span class="txn-amount">₹{transaction.amount}/-</span>
        </label>
      {/each}
      {#if $matchingTransactionsQuery.hasNextPage !== false}
        <button
          class="load-more-btn"
          on:click={loadMore}
          disabled={loadingMore}
        >
          {loadingMore ? "Loading…" : "Load more"}
        </button>
      {/if}
    </div>

    {#if Object.values(selectedTransactions).filter(x => x.state).length > 0}
    <label for="record-bill-payment-amount">Total Amount (calculated from selected transactions)</label>
    <p class="total-amount">{`₹ ${amount}/-`}</p> 
    {/if}

    <div class="checkbox-group">
      <label for="record-bill-payment-fullypaid">Fully Paid</label>
      <input
        type="checkbox"
        bind:checked={isFullyPaid}
        name="record-bill-payment-fullypaid"
        id="record-bill-payment-fullypaid"
      />
    </div>

  <Button
    onclick={async () => {
      isSaving = true;
      await onRecordingPayment({
      id: selectedStatement.id,
      amount:  Object.values(selectedTransactions).filter(x => x.state).length > 0 ? amount : +transactionSearchTerm,
        bill,
      cycleFromDate: selectedStatement.startDate,
        cycleToDate:selectedStatement.endDate,
        isFullyPaid,
        transactions: Object.values(selectedTransactions).filter(x => x.state).map((p: IsSelected<TransactionDTO>) => p.metadata)
      });
      isSaving = false;
    }}
    state={isSaving ? "LOADING" : "DEFAULT"}
    >Record Payment
  </Button>
  <!-- {/if} -->
  </div>
</div>

<style>
  .container {
    height: 100%;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    overflow-y: scroll;
  }

  .form-container {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    padding-bottom: 3rem;
  }

  .form-container h2 {
    font-family: "Syne", sans-serif;
    font-size: 1rem;
    font-weight: 700;
    letter-spacing: -0.01em;
    color: var(--color-base-content);
    margin: 0 0 0.5rem 0;
  }

  .form-container label {
    font-family: "Syne", sans-serif;
    font-size: 0.6875rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.07em;
    color: var(--color-neutral-content);
    margin-top: 0.5rem;
  }

  .bill-statement {
    margin: 0.25rem 0;
  }

  .bill-statement--locked {
    opacity: 0.3;
  }

  .bill-statement--selected {
    opacity: 1;
    border: 2px solid var(--color-primary);
    border-radius: 0.5rem;
  }

  input[type="text"],
  input:not([type="checkbox"]) {
    background-color: var(--color-base-300);
    border: 1px solid #2a2a38;
    border-radius: 0.375rem;
    color: var(--color-base-content);
    padding: 0.625rem 0.75rem;
    font-family: "JetBrains Mono", monospace;
    font-size: 1.25rem;
    font-weight: 600;
    width: 100%;
    transition: border-color 0.15s ease;
  }

  input:not([type="checkbox"]):focus {
    outline: none;
    border-color: var(--color-primary);
  }

  input::placeholder {
    color: #3a3a50;
    font-family: "DM Sans", sans-serif;
    font-size: 0.875rem;
    font-weight: 400;
  }

  .checkbox-group {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 0;
  }

  .checkbox-group label {
    margin-top: 0;
    font-size: 0.8125rem;
    text-transform: none;
    letter-spacing: 0;
    color: var(--color-base-content);
  }

  .total-amount {
    font-family: "JetBrains Mono", monospace;
    font-size: 1.5rem;
    font-weight: 700;
    color: var(--color-primary);
    margin: 0.25rem 0;
  }

  .txn-list {
    display: flex;
    flex-direction: column;
    gap: 0;
    border: 1px solid var(--color-base-300);
    border-radius: 0.5rem;
    overflow: hidden;
  }

  .txn-row {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.625rem 0.75rem;
    border-bottom: 1px solid var(--color-base-300);
    cursor: pointer;
    transition: background-color 0.1s ease;
    text-transform: none;
    letter-spacing: 0;
    margin-top: 0;
  }

  .txn-row:last-of-type {
    border-bottom: none;
  }

  .txn-row input[type="checkbox"] {
    width: 1rem;
    height: 1rem;
    flex-shrink: 0;
    accent-color: var(--color-primary);
  }

  .txn-row--selected {
    background-color: rgba(0, 212, 184, 0.06);
  }

  .txn-details {
    display: flex;
    flex-direction: column;
    gap: 0.125rem;
    flex: 1;
    min-width: 0;
  }

  .txn-merchant {
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--color-base-content);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    font-family: "DM Sans", sans-serif;
  }

  .txn-date {
    font-size: 0.6875rem;
    color: var(--color-neutral-content);
    font-family: "DM Sans", sans-serif;
  }

  .txn-amount {
    font-family: "JetBrains Mono", monospace;
    font-size: 0.8125rem;
    font-weight: 600;
    color: var(--color-base-content);
    white-space: nowrap;
    flex-shrink: 0;
  }

  .txn-row--selected .txn-amount {
    color: var(--color-primary);
  }

  .load-more-btn {
    width: 100%;
    padding: 0.625rem;
    background: transparent;
    border: none;
    border-top: 1px solid var(--color-base-300);
    color: var(--color-neutral-content);
    font-family: "Syne", sans-serif;
    font-size: 0.75rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    cursor: pointer;
  }

  .load-more-btn:disabled {
    opacity: 0.5;
    cursor: default;
  }
</style>
