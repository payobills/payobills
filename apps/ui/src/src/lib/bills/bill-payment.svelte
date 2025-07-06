<script lang="ts">
  import { goto } from "$app/navigation";
  import Card from "$lib/card.svelte";
  import { onMount } from "svelte";
  import { withOrdinalSuffix } from "../../utils/ordinal-suffix";

  export let bill;

  let todaysDay: number;
  let billDueDetails;

  onMount(() => {
    todaysDay = new Date().getDate();
  });

  $: {
    const diffInDays = bill.payByDate - todaysDay;
    if (diffInDays > 0)
      billDueDetails = { string: `Due in ${diffInDays} days`, status: "due", l2Status: diffInDays <= 5 ? 'warning':'ok' };
    else if (diffInDays < 0)
      billDueDetails = {
        string: `Overdue by ${-diffInDays} days`,
        status: "overdue"
      };
    else billDueDetails = { string: "Due today", status: "today" };
  }
</script>

<div class="container">
  <div class="header">
    <div class="name">{bill.name}</div>
    <div class="actions">
      <button
        on:click={() => goto(`bills/${bill.id}`)}
        >Pay now
      </button>
    </div>
  </div>

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
    Current bill status <span class={`due-status due-status--${billDueDetails?.status} ${billDueDetails?.l2Status? `due-status--${billDueDetails?.status}--${billDueDetails?.l2Status}`: ''}`}><strong>{billDueDetails?.string}</strong></span>
  </div>

  <!-- <button
    on:click={() => goto(`bills/${bill.id}`)}
    class={`due-status due-status--${billDueDetails?.status}`}
  >
    
  </button> -->
  <!-- <PaymentTimelinePill {item} /> -->
  <!-- <div>{JSON.stringify(bill)}</div> -->
</div>

<style>
  .container {
    background-color: rgb(233, 233, 233);
    border-radius: 0.125rem;
    border: 0.125rem solid rgb(216, 216, 216);
    /* box-shadow: 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24); */
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
    margin: .5rem 1rem 0.5rem 1rem;
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
    height: 0.0625rem;
    border: none;
    background-color: rgb(216, 216, 216);
    margin: 0;
  }

  button { margin: .5rem; padding: .5rem;}

  .due-status {
    padding: .25rem;
    min-width: 40%;
    text-align: center;
    /* padding: 0.25rem; */
    border-radius: 0.55rem;
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
    background-color: red;
  }

  .due-status--today {
    background-color: orange;
  }
</style>
