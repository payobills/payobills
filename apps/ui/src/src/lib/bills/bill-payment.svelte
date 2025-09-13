<script lang="ts">
  import { goto } from "$app/navigation";
  import Card from "$lib/card.svelte";
  import { onMount } from "svelte";
  import { withOrdinalSuffix } from "../../utils/ordinal-suffix";
  import Page from "../../routes/+page.svelte";
  import { getBillPaymentCycle } from "../../utils/get-bill-payment-cycle";
  import { fromStore } from "svelte/store";
  import { uiDrawer } from "$lib/stores/ui-drawer";
  import RecordPaymentForm from "$lib/record-payment-form.svelte";
  import UiDrawer from "$lib/ui-drawer.svelte";

  export let bill;
  export let billingStatements: any[] | undefined;
  export let showRecordPaymentButton = true;
  export let title = "";
  export let onRecordingPayment: any;

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
        cycle?.toDate === statement.endDate
    );

    currentCycleFromDate = cycle?.fromDate || "";
    currentCycleToDate = cycle?.toDate || "";
    const diffInDays = bill.payByDate - todaysDay;

    if (!billingStatements) {
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
    <div class="name">{title?.length > 0 ? title : bill.name}</div>
    {#if showRecordPaymentButton}
      <div class="actions">
        <button
          on:click={() => {
            goto(`#payment__bill_${bill.id}`);
            currentPayingBill = null;
            currentPayingBill = bill;
            console.log("currentPayingBill", currentPayingBill);
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
      <RecordPaymentForm bill={currentPayingBill} {onRecordingPayment} />
    </UiDrawer>
  {/if}

  <div class="card-item">
    Bill generates <strong>{withOrdinalSuffix(bill.billingDate)}</strong> of every
    month
  </div>
  <div class="card-item">
    Bill payment due <strong>{withOrdinalSuffix(bill.payByDate)}</strong> of every
    month
  </div>

  <div class="card-item">
    Billing cycle: {currentCycleFromDate} - {currentCycleToDate}
  </div>
  <div class="card-item">
    Current bill status <span
      class={`due-status due-status--${billDueDetails?.status} ${billDueDetails?.l2Status ? `due-status--${billDueDetails?.status}--${billDueDetails?.l2Status}` : ""}`}
      ><strong>{billDueDetails?.string}</strong></span
    >
  </div>
  {#if currentBillStatement?.amount > 0}
    <div class="card-item">
      Total Payment made this cycle: <span class="current-bill-amount"
        >₹{currentBillStatement?.amount}/-</span
      >
    </div>
  {/if}
</div>

<style>
  .current-bill-amount {
    font-weight: 800;
  }

  .container {
    /* background-color: rgb(233, 233, 233); */
    border-radius: 0.425rem;
    background-color: rgb(59, 59, 59);
    /* border: 0.125rem solid rgb(216, 216, 216); */
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    align-items: top;
    margin-top: 1rem;
  }

  strong {
    color:white;
  }

  .container > div:nth-of-type(2) {
    margin-top: 1rem;
  }

  .container > div:last-of-type {
    margin-bottom: 1rem;
  }

  .header {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
  }

  .name {
    font-size: 1rem;
    font-weight: 400;
    margin: 0.5rem 1rem 0.5rem 1rem;
    font-weight: 900;
  }

  strong {
    font-weight: 800;
  }

  .card-item {
    margin: 0.5rem 1rem;
    font-size: 0.75rem;
  }

  hr {
    width: 100%;
    height: 0.125rem;
    border: none;
    background-color: rgb(216, 216, 216);
    margin: 0;
  }

  button {
    margin: 0.5rem;
    padding: 0.5rem;
  }

  .due-status {
    padding: 0.25rem 0.5rem;
    min-width: 40%;
    text-align: center;
    /* padding: 0.25rem; */
    border-radius: 1rem;
    /* margin: 0.5rem 1rem 1rem 1rem; */
  }

  .due-status--due {
    color: white;
    background-color: var(--primary-accent-color);
  }

  .due-status--paid {
    color: white;
    background-color: rgb(9, 174, 9);
  }

  .due-status--loading {
    color: white;
    background-color: rgb(55, 55, 55);
  }

  .due-status--due--warning {
    color: white;
    background-color: rgb(225, 153, 20);
  }

  .due-status--overdue {
    color: white;
    background-color: red;
  }

  .due-status--today {
    color: white;
    background-color: orange;
  }
</style>
