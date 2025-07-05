<script lang="ts">
  import { goto } from "$app/navigation";
  import Card from "$lib/card.svelte";
  import { onMount } from "svelte";

  export let bill;

  let todaysDay: number;
  let billDueDetails;

  onMount(() => {
    todaysDay = new Date().getDate();
  });

  $: {
    const diffInDays = bill.payByDate - todaysDay;
    if (diffInDays > 0)
      billDueDetails = { string: `Due in ${diffInDays} days`, status: "due" };
    else if (diffInDays < 0)
      billDueDetails = {
        string: `Overdue by ${-diffInDays} days`,
        status: "overdue",
      };
    else billDueDetails = { string: "Due today", status: "today" };
  }
</script>

<div class="container">
  <div>{bill.name}</div>
  <button
    on:click={() => goto(`bills/${bill.id}`)}
    class={`bill due-status due-status--${billDueDetails?.status}`}
  >
    {billDueDetails?.string}
  </button>
  <!-- <PaymentTimelinePill {item} /> -->
  <!-- <div>{JSON.stringify(bill)}</div> -->
</div>

<style>
  .card-container {
    border: 1px solid grey 0.25rem;
    box-shadow: 0 3px 3px 1px rgba(0, 0, 0, 0.16);
  }

  .container {
    background-color: #e0e0e0;
    padding: 1rem;
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    margin-bottom: .5rem;
  }

  .due-status {
    min-width: 40%;
    text-align: center;
    /* padding: 0.25rem; */
    border-radius: 0.125rem;
  }

  .due-status--due {
    color: white;
    background-color: var(--primary-accent-color);
  }

  .due-status--overdue {
    background-color: red;
  }

  .due-status--today {
    background-color: orange;
  }
</style>
