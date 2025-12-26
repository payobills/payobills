<script lang="ts">
  import type { Writable } from "svelte/store";
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

  export let onTransactionSearch: (transactionSearchTerm: string) => Writable<Query<TransactionDTO[]>>

  let matchingTransactionsQuery: Writable<Query<TransactionDTO[]>> 

  let transactionSearchTerm: string = ''
  $: selectedTransactions = ($matchingTransactionsQuery?.data?.reduce((acc, curr) => { 
        acc = {...acc, [curr.id.toString()]: new IsSelected(false, curr) } 
        return acc
      }, {}) ?? {}) satisfies Record<string, IsSelected<TransactionDTO>> 
  $: amount = Object.values(selectedTransactions).filter(x => x.state).reduce((acc, curr) => acc + curr.metadata?.amount || 0, 0)

  $: {
    console.log('selec', selectedStatement)
  }

  let isFullyPaid = true;
  let isSaving = false;

  class IsSelected<T> {
    constructor(private __state: boolean, private __metadata: T) { }
    get state() { return this.__state }
    set state(newState: boolean) { this.__state = newState}
    get metadata(): T { return this.__metadata}
  }

  const onAmountSearchBoxFocusout = async () => {
    try {
        // console.log('searching,', )
      matchingTransactionsQuery = await onTransactionSearch(transactionSearchTerm)
    }
    catch {}
  }
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
      onfocusout={onAmountSearchBoxFocusout}
    />

    <!-- {#if matchingTransactions.length > 0 } -->
    <!-- {#each [{ amount: 377, merchant: "test merchant"}] as transaction (transaction.id)} -->
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
    <!-- </if} -->
    <!-- <label for="record-bill-transactions"></label> -->

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
    <!-- {#if Object.values(selectedTransactions).filter(x => x.state).length > 0} -->
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
    justify-content: space-between;
    gap: 1rem;
    border-radius: 8px;
    background-color: var(--color-background);
    box-shadow: var(--shadow-elevation-2);
    overflow-y: scroll;
  }

  .form-container {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .form-container:last-child {
    margin-bottom: 4rem;
  }
  
  .bill-statement {
    margin: .25rem 0
  }

  .bill-statement--locked {
    /* border: .25rem solid var(--primary-accent-color); */
    /* border-radius: 0.425rem; */
    opacity: .3;
  }

  .bill-statement--selected {
    opacity: 1;
    border: .25rem solid var(--primary-accent-color);
    border-radius: 0.425rem;
  }

  input {
    border: 0;
    border-radius: 0.25rem;
    padding: 0.5rem;
    font-size: 1.5rem;
  }

  .checkbox-group {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }

  .total-amount {
    margin-left: 1rem;
    font-size: 2rem
  }
</style>
