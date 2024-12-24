<script lang="ts">
  import { goto } from "$app/navigation";
  import RecentTransactions from "$lib/recent-transactions.svelte";
  import { paymentsUrql } from "$lib/stores/urql";
  import {
    faChevronLeft,
    faChevronRight,
    faEdit,
    faPencil,
  } from "@fortawesome/free-solid-svg-icons";

  import { queryStore, gql } from "@urql/svelte";
  import { onMount } from "svelte";
  import { Icon } from "svelte-awesome";
  import { formatRelativeDate } from "../../../utils/format-relative-date";
  import { formStoreGenerator, type FormStore } from "$lib/stores/form";
  import IconButton from "$lib/icon-button.svelte";
  import type { Writable } from "svelte/store";
  import type { Transaction } from "$lib/types/transaction";

  let transactionID: string | null = null;
  let transactionForm: Writable<FormStore<Transaction>>;

  let pageMode: "VIEW" | "EDIT" = "VIEW";

  onMount(() => {
    let path = window.location.pathname;
    transactionID = path.split("/")[2];
    transactionForm = formStoreGenerator("transactionById");
  });

  $: transactionsQuery = queryStore({
    client: $paymentsUrql,
    variables: { id: transactionID },
    query: gql`
      query ($id: String!) {
        transactionByID(id: $id) {
          id
          amount
          merchant
          backDate
          transactionText
          tags
          notes
        }
      }
    `,
  });

  $: {
    if ($transactionsQuery?.data?.transactionByID)
      {
        console.log('updated transcation form store')
        transactionForm.set({data: $transactionsQuery.data.transactionByID, isDirty: false});
      }
  }
</script>

{#if $transactionsQuery.fetching}
  <p>Loading...</p>
{:else if $transactionsQuery.error}
  <p>🙆‍♂️ Uh oh! Unable to fetch your bills!</p>
{:else}
  <div class="transaction">
    {#if pageMode === "VIEW"}
      <section class="title">
        <div class="amount">
          {#if $transactionsQuery.data.transactionByID.amount !== null}
            <span class="value"
              >{new Intl.NumberFormat(undefined, {
                style: "currency",
                currency: "INR",
              }).format($transactionsQuery.data.transactionByID.amount)}</span
            >
          {:else}
            <span class="value">Unknown Amount</span>
          {/if}

          <IconButton
            icon={faEdit}
            backgroundColor="var(--primary-bg-color)"
            color="black"
            scale={1.5}
            on:click={() => (pageMode = "EDIT")}
          />
        </div>
        <div class="transaction-detail">
          {formatRelativeDate(
            new Date($transactionsQuery.data.transactionByID.backDate)
          )} • {new Intl.DateTimeFormat("en-GB", {
            year: "2-digit",
            month: "long",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
          }).format(new Date($transactionsQuery.data.transactionByID.backDate))}
        </div>
      </section>
      <section class="content">
        {#if $transactionsQuery.data.transactionByID.merchant !== null}
          <h1>Recipient</h1>
          <h2>{$transactionsQuery.data.transactionByID.merchant}</h2>
        {:else}
          <h1>Recipient</h1>
          <h2>Unknown</h2>
        {/if}

        {#if $transactionsQuery.data.transactionByID.transactionText !== ""}
          <h1 class="subheader">Original Transaction Detail</h1>
          <div class="transaction-detail">
            {$transactionsQuery.data.transactionByID.transactionText}
          </div>
        {/if}

        <h1 class="subheader">Tags</h1>
        <div class="tags">
          {#each $transactionsQuery.data.transactionByID.tags as tag}
            <div class="tag">{tag}</div>
          {/each}
        </div>

        <h1 class="subheader">Notes</h1>
        <p class="note note__view">
          {$transactionsQuery.data.transactionByID.notes ||
            "This transaction doesn't have a note."}
        </p>
      </section>
      <!-- REGION: EDIT -->
    {:else}
      <section class="title title__edit">
        {#if $transactionForm.data?.amount !== null}
          <input
            class="amount amount__edit"
            placeholder="Unknown Amount"
            bind:value={($transactionForm.data as Transaction).amount}
          />
        {/if}
        <div class="transaction-detail">
          {formatRelativeDate(
            new Date($transactionsQuery.data.transactionByID.backDate)
          )} • {new Intl.DateTimeFormat("en-GB", {
            year: "2-digit",
            month: "long",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
          }).format(new Date($transactionsQuery.data.transactionByID.backDate))}
        </div>
      </section>
      <section class="content">
        <h1>Recipient</h1>
        <input bind:value={($transactionForm.data as Transaction).merchant} />

        <h1 class="subheader">Original Transaction Detail</h1>
        <div class="transaction-detail">
          {$transactionsQuery.data.transactionByID.transactionText}
        </div>

        <h1 class="subheader">Tags</h1>
        <div class="tags">
          {#each $transactionsQuery.data.transactionByID.tags as tag}
            <div class="tag">{tag}</div>
          {/each}
        </div>

        <h1 class="subheader">Notes</h1>
        <textarea
          class="note note__edit"
          value={$transactionsQuery.data.transactionByID.notes}
          placeholder="This transaction doesn't have a note. Click here to add one."
        ></textarea>
        <button class="submit">save</button>
        <button class="submit" on:click={() => (pageMode = "VIEW")}
          >cancel</button
        >
      </section>
    {/if}
  </div>
{/if}

<style>
  .submit {
    margin-top: 1rem;
  }

  .note {
    margin: 0;
    margin-top: 1rem;
    font-size: 0.75rem;
    border-radius: 0.25rem;
    align-self: stretch;
    flex-grow: 1;
    border: none;
    font-family: "Courier New", Courier, monospace;
  }
  .note__edit {
    background-color: #e2e2e2;
  }
  .note__view {
    background-color: var(--primary-bg-color);
  }

  .content {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }

  .transaction {
    margin: 1rem;
    height: 100%;
    display: flex;
    flex-direction: column;
  }

  .transaction-detail {
    font-size: 0.75rem;
    font-weight: 400;
    margin: 1rem auto;
  }

  p {
    margin: 0.75rem 1rem;
  }

  .title {
    background-color: var(--primary-bg-color);
    margin-bottom: 1rem;
  }

  h1 {
    margin: 0;
  }

  h2 {
    font-size: 1rem;
  }

  .amount {
    margin: 1rem 0;
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-size: 2rem;
  }

  .amount__edit {
    width: 100%;
  }
  .tags {
    display: flex;
    margin: 1rem 0;
    row-gap: 0.5rem;
    column-gap: 0.5rem;
    flex-wrap: wrap;
  }

  .tag {
    background-color: gray;
    border-radius: 5px;
    color: white;
    padding: 0.25rem;
  }
</style>
