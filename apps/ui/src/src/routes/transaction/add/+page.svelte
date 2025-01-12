<script lang="ts">
  import { gql, queryStore } from "@urql/svelte";

  const onSubmit = () => {};

  import { billsUrql } from "$lib/stores/urql";

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
</script>

<section class="add-transaction">
  {#if $billsQuery.fetching}
    <p>Loading...</p>
  {:else if $billsQuery.error}
    <p>🙆 Uh oh! Looks like we're out at the moment! Try again soon.</p>
  {:else}
    <h1 class="title">Add a transaction</h1>

    <form on:submit|preventDefault={onSubmit}>
      <label for="bill"
        >Which bill should this transaction be associated to?</label
      >
      <select class="bill-select" id="bill">
        {#each $billsQuery.data.bills as bill}
          <option value={bill.id}>{bill.name}</option>
        {/each}
      </select>

      <label for="amount"> Amount </label>
      <input type="number" id="amount" placeholder="100" />

      <label for="notes"> Additional notes </label>
      <textarea
        id="notes"
        placeholder="Add more details to remember about this transaction."
      ></textarea>

      <button type="submit">Add transaction</button>
    </form>
  {/if}
</section>

<style>
  .add-transaction {
    margin: 1rem;
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

  button {
    color: var(--primary-bg-color);
  }
</style>
