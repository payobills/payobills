<script lang="ts">
  import { page } from "$app/stores";
  import { paymentForm } from "$lib/stores/payment-form";
  import { billsUrql } from "$lib/stores/urql";
  import { queryStore, gql } from "@urql/svelte";
  import { onMount } from "svelte";

  let billId: any;
  let billByIdQuery: any;
  let refreshKey: number = Date.now();
  let form = $paymentForm;
  onMount(() => {
      billId = $page.url.searchParams.get("bill-id");
    paymentForm.update((value) => (value.inputs.billId = billId));
  });

  $: billByIdQuery = queryStore({
    client: $billsUrql,
    query: gql`
      query billById($billId: String!) {
        billById(id: $billId) {
          id
          name
          billingDate
          payByDate
          updatedAt
          createdAt
          payments {
            id
            amount
            paidAt
            billingPeriod {
              start
              end
            }
            createdAt
            updatedAt
          }
        }
      }
    `,
    variables: { billId, refreshKey },
  });

//   $: console.table(form);
</script>

<div class="content">
  {#if $billByIdQuery.fetching}
    <p>Loading...</p>
  {:else if $billByIdQuery.error}
    <p>🙆‍♂️ Uh oh! Unable to fetch your bill!</p>
  {:else}
    <p>
      You are making a payment for <strong
        >{$billByIdQuery.data.billById.name}</strong
      >
    </p>

    <form action="">
      <p>Amount paid <sup><span class="required">* required</span></sup></p>
      <input
        type="text"
        placeholder="Enter amount"
        bind:value={form.inputs.amount}
      />

      <p>Bill month <sup><span class="required">* required</span></sup></p>
      <input type="month" bind:value={form.inputs.billMonthYear} />

      <p>Estimated bill period</p>
      <p>
        {form.calculatedValues.billPeriodStart} to {form
          .calculatedValues.billPeriodEnd}
      </p>

      <p>Backdate (optional)</p>
      <input type="date" />

      <p>Notes(optional)</p>
      <textarea name="notes" id="notes"></textarea>

      <!-- <div class="actions"> -->
      <button type="button">Save</button>
      <!-- <button type="button">Cancel</button> -->
      <!-- </div> -->
    </form>
  {/if}
</div>

<style>
  .content {
    margin: 1rem;
    display: flex;
    flex-direction: column;
    height: 100%;
  }

  strong {
    font-weight: 800;
  }

  input {
    margin: 0.5rem 0;
    width: 100%;
  }

  p {
    margin: 0;
  }

  form {
    textarea {
      width: 100%;
    }

    /* .actions {
            width: 100%;
            display: flex;
            justify-content: space-between;
        } */

    button {
      width: 100%;
      align-self: flex-end;
    }

    span.required {
      color: red;
      font-weight: 400;
      font-size: 0.5rem;
    }
  }
</style>
