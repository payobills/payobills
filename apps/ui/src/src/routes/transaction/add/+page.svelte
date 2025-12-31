<script lang="ts">
  import { gql, queryStore } from "@urql/svelte";
  import { goto, afterNavigate } from "$app/navigation";
  import { billsUrql, paymentsUrql } from "$lib/stores/urql";
    import { onMount } from "svelte";
    import { nav } from "$lib/stores/nav";

  let transactionText = "";
  let billId = "";
  let amount: number | null = null;
  let merchant = "";
  let notes: string | null = null;

  onMount(() => {
    nav.prev(prev => ({...prev, isOpen: true})) 
    })

  const billsQuery = queryStore({
    client: $billsUrql,
    query: gql`
      query {
        bills {
          id
          name
          billingDate
          payByDate
          isEnabled
        }
      }
    `,
  });

  const addTransaction = () => {
    let client;
    try {
      $paymentsUrql
        .mutation(
          gql`
            mutation ($input: TransactionAddDTOInput!) {
              transactionAdd(input: $input) {
                id
              }
            }
          `,
          {
            input: {
              ...(amount && { amount }),
              transactionText,
              parseStatus: "NotStarted",
              merchant,
              bill: { id: +billId },
              notes: notes || ""
            },
          }
        )
        .toPromise()
        .then(() => {
          goto("/");
        });
    } catch (error) {
      console.error("couldn't get client", error);
    }
  };
</script>

<section class="add-transaction">
  {#if $billsQuery.fetching}
    <p>Loading...</p>
  {:else if $billsQuery.error}
    <p>🙆 Uh oh! Looks like we're out at the moment! Try again soon.</p>
  {:else}
    <h1 class="title">Add a transaction</h1>

    <form on:submit|preventDefault={addTransaction}>
      <label for="bill"
        >Which bill should this transaction be associated to?</label
      >
      <select bind:value={billId} class="bill-select" id="bill">
        {#each $billsQuery.data.bills as bill}
          <option value={bill.id}>{bill.name}</option>
        {/each}
      </select>

      <label for="notes"> Transaction Text </label>
      <textarea
        id="notes"
        placeholder="Paste the transaction confirmation text from SMS (optional)"
        bind:value={transactionText}
      ></textarea>

      <label for="amount"> Amount </label>
      <input bind:value={amount} type="number" step="0.000001" id="amount" placeholder="100" />

      <label for="merchant"> Merchant </label>
      <input
        bind:value={merchant}
        type="text"
        id="merchant"
        placeholder="Thirdwave Coffee"
      />

      <label for="notes"> Additional notes </label>
      <textarea
        id="notes"
        placeholder="Add more details to remember about this transaction."
        bind:value={notes}
      ></textarea>

      <button type="submit">Add transaction</button>
    </form>
  {/if}
</section>

<style>
  .add-transaction {
    padding: 1rem;
    height: 100%;
    display: flex;
    flex-direction: column;
    overflow-y: scroll;
  }
  .title {
    margin: 0;
  }

  .bill-select {
    width: 100%;
  }

  form {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }

  form > * {
    margin: 0.5rem 0;
  }

  label {
    display: block;
    font-family: 0.75rem;
  }

  input,
  textarea {
    border: none;
  }

  textarea {
    flex-grow: 1;
    min-height: 10rem;
    max-height: 15rem;
    resize: none;
  }
  button {
    color: var(--primary-bg-color);
    /* display: flex; */
    /* justify-self: flex-end; */
  }
</style>
