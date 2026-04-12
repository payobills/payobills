<script lang="ts">
import { goto } from "$app/navigation";
import Card from "$lib/card.svelte";
import { onMount } from "svelte";
import { withOrdinalSuffix } from "../../utils/ordinal-suffix";
import { getBillPaymentCycle } from "../../utils/get-bill-payment-cycle";
import { fromStore } from "svelte/store";
import { uiDrawer } from "$lib/stores/ui-drawer";
import RecordPaymentForm from "$lib/record-payment-form.svelte";
import UiDrawer from "$lib/ui-drawer.svelte";
import type { BillStatementDTO } from "$lib/types";

export let bill;
export let billingStatements: BillStatementDTO[] | undefined;
export let showRecordPaymentButton = true;
export let title = "";
export let showBillingCycle = true;

export let onRecordingPayment: any;
export let onCurrentBillStatementDoesNotExist: any;
export let onTransactionSearch: any;

let todaysDay: number;
let billDueDetails: { status: string; string: string; l2Status?: string };

export let currentCycleFromDate = "";
export let currentCycleToDate = "";

let currComponent: HTMLDivElement;
let currentPayingBill: any;
let currentBillStatement: any;

onMount(() => {
  todaysDay = new Date().getDate();
});

$: {
  // TODO: Prepaid type of bills will have cycle with current date in the cycle
  // Add edge case once Type field is exposed on BillDTO
  const cycle = getBillPaymentCycle(bill);

  currentBillStatement = billingStatements?.find(
    (statement) =>
      statement.startDate === cycle?.fromDate &&
      cycle?.toDate === statement.endDate,
  );

  if (!currentBillStatement) {
    currentBillStatement = {
      startDate: cycle?.fromDate,
      endDate: cycle?.toDate,
      amount: undefined,
    };
  }

  currentCycleFromDate = cycle?.fromDate || "";
  currentCycleToDate = cycle?.toDate || "";
  const diffInDays = bill.payByDate - todaysDay;

  if (!bill.isEnabled) {
    billDueDetails = {
      string: "Disabled",
      status: "disabled",
    };
  } else if (!billingStatements) {
    billDueDetails = {
      string: `Calculating ...`,
      status: "loading",
    };
  } else {
    if (
      currentBillStatement?.isFullyPaid &&
      currentBillStatement?.amount === 0
    ) {
      billDueDetails = {
        string: `No payment due`,
        status: "paid",
      };
    } else if (currentBillStatement?.isFullyPaid) {
      billDueDetails = {
        string: `Fully Paid`,
        status: "paid",
      };
    } else if (diffInDays > 0)
      billDueDetails = {
        string: `Due in ${diffInDays} days`,
        status: "due",
        l2Status: diffInDays <= 5 ? "warning" : "ok",
      };
    else if (diffInDays < 0)
      billDueDetails = {
        string: `Overdue by ${-diffInDays} days`,
        status: "overdue",
      };
    else billDueDetails = { string: "Due today", status: "today" };
  }
}
</script>

<div class="container" bind:this={currComponent}>
  <a href={`#payment__bill_${bill.id}`} aria-label="anchor"></a>
  <div class="header">
    <div class="name">
      {title?.length > 0 ? title : bill.name}
      {#if bill.primaryType}
        <span class="bill-type">{bill.primaryType}</span>
      {/if}
    </div>
    {#if bill.isEnabled && showRecordPaymentButton}
      <div class="actions">
        <button
          on:click={() => {
            goto(`#payment__bill_${bill.id}`);
            currentPayingBill = null;
            currentPayingBill = bill;
            // console.log("currentPayingBill", currentPayingBill);
          }}>Record payment</button
        >
      </div>
    {/if}
  </div>

  {#if currentPayingBill}
    <UiDrawer
      onClose={() => {
        currentPayingBill = null;
      }}
    >
      <RecordPaymentForm bill={currentPayingBill}
        {onRecordingPayment}
        billStatements={[currentBillStatement]}
        selectedStatement={currentBillStatement}
        {onTransactionSearch}
      />
    </UiDrawer>
  {/if}

  <div class="card-item">
    Bill generates <strong>{withOrdinalSuffix(bill.billingDate)}</strong> of every
    month and due <strong>{withOrdinalSuffix(bill.payByDate)}</strong> of every
    month
  </div>

  {#if showBillingCycle}
    <div class="card-item">
      Billing cycle: {currentCycleFromDate} - {currentCycleToDate}
    </div>
  {/if}

  <div class="card-item">
    Current bill status <span
      class={`due-status due-status--${billDueDetails?.status} ${billDueDetails?.l2Status ? `due-status--${billDueDetails?.status}--${billDueDetails?.l2Status}` : ""}`}
      ><strong>{billDueDetails?.string}</strong></span
    >
  </div>
    <div class="card-item">
      {#if currentBillStatement?.amount > 0} Total Payment made this cycle: <span class="current-bill-amount"
        >₹{currentBillStatement?.amount}/-</span>
      {:else}
      No payment was required in this cycle.
{/if}
    </div>
</div>

<style>
  .container {
    border-radius: 0.625rem;
    background-color: var(--color-base-200);
    border: 1px solid var(--color-base-300);
    display: flex;
    flex-direction: column;
    margin-top: 0.625rem;
    width: 100%;
    transition: border-color 0.2s ease;
    overflow: hidden;
  }

  .container:hover {
    border-color: rgba(0, 212, 184, 0.2);
  }

  .header {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 0.875rem 0;
    border-bottom: 1px solid var(--color-base-300);
    padding-bottom: 0.625rem;
  }

  .name {
    font-family: "Syne", system-ui, sans-serif;
    font-size: 0.9375rem;
    font-weight: 700;
    letter-spacing: -0.01em;
    color: var(--color-base-content);
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }

  .bill-type {
    font-family: "DM Sans", sans-serif;
    font-size: 0.6rem;
    font-weight: 600;
    color: var(--color-neutral-content);
    background-color: var(--color-base-300);
    border: 1px solid #2a2a38;
    padding: 0.15rem 0.45rem;
    border-radius: 0.25rem;
    text-transform: uppercase;
    letter-spacing: 0.06em;
  }

  strong {
    font-weight: 700;
    color: var(--color-base-content);
  }

  .card-item {
    margin: 0;
    padding: 0.4rem 0.875rem;
    font-size: 0.75rem;
    color: var(--color-neutral-content);
    border-bottom: 1px solid rgba(28, 28, 38, 0.5);
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.5rem;
  }

  .card-item:last-of-type {
    border-bottom: none;
    padding-bottom: 0.625rem;
  }

  .current-bill-amount {
    font-family: "JetBrains Mono", monospace;
    font-weight: 700;
    font-size: 0.875rem;
    color: var(--color-primary);
  }

  button {
    margin: 0.5rem 0.625rem 0.5rem 0;
    padding: 0.3rem 0.875rem;
    border-radius: 0.375rem;
    font-size: 0.7rem;
    font-weight: 600;
    letter-spacing: 0.03em;
    cursor: pointer;
    background-color: rgba(0, 212, 184, 0.1);
    border: 1px solid rgba(0, 212, 184, 0.25);
    color: var(--color-primary);
    text-transform: none;
    transition: all 0.15s ease;
  }

  button:hover {
    background-color: rgba(0, 212, 184, 0.18);
    border-color: rgba(0, 212, 184, 0.5);
  }

  .due-status {
    padding: 0.2rem 0.625rem;
    border-radius: 0.3rem;
    font-size: 0.7rem;
    font-weight: 600;
    letter-spacing: 0.03em;
    text-transform: uppercase;
    font-family: "DM Sans", sans-serif;
  }

  .due-status--due {
    color: #38bdf8;
    background-color: rgba(56, 189, 248, 0.12);
    border: 1px solid rgba(56, 189, 248, 0.25);
  }

  .due-status--disabled {
    color: #4a5568;
    background-color: rgba(26, 26, 38, 0.5);
    border: 1px solid #2a2a38;
  }

  .due-status--paid {
    color: #22d3a0;
    background-color: rgba(34, 211, 160, 0.12);
    border: 1px solid rgba(34, 211, 160, 0.25);
  }

  .due-status--loading {
    color: #4a5568;
    background-color: rgba(26, 26, 38, 0.5);
    border: 1px solid #2a2a38;
  }

  .due-status--due--warning {
    color: #fbbf24;
    background-color: rgba(251, 191, 36, 0.12);
    border: 1px solid rgba(251, 191, 36, 0.25);
  }

  .due-status--overdue {
    color: #f43f5e;
    background-color: rgba(244, 63, 94, 0.12);
    border: 1px solid rgba(244, 63, 94, 0.25);
  }

  .due-status--today {
    color: #fb923c;
    background-color: rgba(251, 146, 60, 0.12);
    border: 1px solid rgba(251, 146, 60, 0.25);
  }
</style>
