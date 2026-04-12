<script lang="ts">
import { writable, type Writable } from "svelte/store";
import BillPayment from "./bills/bill-payment.svelte";
import BottomNav from "./bottom-nav.svelte";
import Button from "./button.svelte";
import Card from "./card.svelte";
import type { BillStatementDTO, Query, TransactionDTO } from "./types";
import { formatRelativeDate } from "../utils/format-relative-date";

export let bill: any;
export let billStatements: BillStatementDTO[];
export let onRecordingPayment;
export let lockBillStatementCycle = false;
export let selectedStatement: BillStatementDTO;

export let onTransactionSearch: any;
let matchingTransactionsQuery: Writable<Query<TransactionDTO[]>> = writable({
  data: [],
  fetching: false,
  error: null,
});

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

    <!-- {#if matchingTransactions.length > 0 } -->
    <!-- {#each [{ amount: 377, merchant: "test merchant"}] as transaction (transaction.id)} -->
    <!-- <pre>{JSON.stringify(())}</pre> -->
    {#each $matchingTransactionsQuery?.data as transaction (transaction.id)}
      <Card>
        <p> 

        <!-- bind:checked={new IsSelected(false).state} -->
      <input
        type="checkbox"
        bind:checked={selectedTransactions[transaction.id].state}
        name={`record-bill-payment-transaction-${transaction.id}`}
        id={`record-bill-payment-transaction-${transaction.id}`}
      />
          <span>{`₹${transaction.amount}/- `}</span>{`at ${transaction.merchant ?? 'Unknown Merchant'} · `}<span>{formatRelativeDate(new Date(transaction.paidAt))}</span>
          </p>
      </Card>
    {/each}

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
</style>
