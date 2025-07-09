<script lang="ts">
  import { goto } from "$app/navigation";
  import Card from "$lib/card.svelte";
  import { onMount } from "svelte";
  import { withOrdinalSuffix } from "../../utils/ordinal-suffix";
  import Page from "../../routes/+page.svelte";
  import { getBillPaymentCycle } from "../../utils/get-bill-payment-cycle";
  import { fromStore } from "svelte/store";
  import { uiDrawer } from "$lib/stores/ui-drawer";
  import BillPaymentForm from "$lib/record-payment-form.svelte";
  import UiDrawer from "$lib/ui-drawer.svelte";

  export let bill;
  export let billingStatements: any[];

  let todaysDay: number;
  let billDueDetails: { status: string; string: string; l2Status?: string };
  let currentCycleFromDate = "";
  let currentCycleToDate = "";

  let currComponent: HTMLDivElement;
  let currentPayingBill: any;

  onMount(() => {
    todaysDay = new Date().getDate();
  });

  $: {
    // TODO: Prepaid type of bills will have cycle with current date in the cycle
    // Add edge case once Type field is exposed on BillDTO
    const cycle = getBillPaymentCycle(bill);
    currentCycleFromDate = cycle?.fromDate || "";
    currentCycleToDate = cycle?.toDate || "";

    const diffInDays = bill.payByDate - todaysDay;
    if (diffInDays > 0)
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
</script>

<div class="container" bind:this={currComponent}>
  <a href={`#payment__bill_${bill.id}`} aria-label="anchor"></a>
  <div class="header">
    <div class="name">{bill.name}</div>
    <div class="actions">
      <button
        on:click={() => {
          goto(`#payment__bill_${bill.id}`);
          currentPayingBill = null;
          currentPayingBill = bill;
          console.log('currentPayingBill',currentPayingBill)
        }}>Record payment</button
      >
    </div>
  </div>

  {#if currentPayingBill}
    <UiDrawer onClose={() => {currentPayingBill = null;}}>
      <h2>Mark a payment for <strong>{bill.name}</strong></h2>
      <BillPaymentForm bill={currentPayingBill} />
    </UiDrawer>
  {/if}

  <hr />
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
</div>

<style>
  .container {
    background-color: rgb(233, 233, 233);
    border-radius: 0.425rem;
    border: 0.125rem solid rgb(216, 216, 216);
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    align-items: top;
    margin-top: 1rem;
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
