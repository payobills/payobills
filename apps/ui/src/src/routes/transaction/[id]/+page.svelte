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

  import { queryStore, gql, mutationStore } from "@urql/svelte";
  import type { OperationResultStore } from "@urql/svelte";
  import { onMount } from "svelte";
  import { Icon } from "svelte-awesome";
  import { formatRelativeDate } from "../../../utils/format-relative-date";
  import { formStoreGenerator, type FormStore } from "$lib/stores/form";
  import IconButton from "$lib/icon-button.svelte";
  import type { Writable } from "svelte/store";
  import type { Transaction } from "$lib/types/transaction";
  import FileUploader from "../../../lib/file-uploader.svelte";

  let transactionID: string | null = null;
  let pageMode: "VIEW" | "EDIT" = "VIEW";
  let transaction: any;
  let transactionsQuery: OperationResultStore;
  let transactionForm: Writable<FormStore<Transaction>> =
    formStoreGenerator("transactionById");

  onMount(() => {
    let path = window.location.pathname;
    transactionID = path.split("/")[2];
    transactionsQuery = queryStore({
      client: $paymentsUrql,
      variables: { id: transactionID },
      query: gql`
        query ($id: String!) {
          transactionByID(id: $id) {
            id
            amount
            merchant
            paidAt
            transactionText
            tags
            parseStatus
            notes
            bill {
              id
              name
            }
          }
        }
      `,
    });

    transactionsQuery.subscribe((res) => {
      if (!res.data) return;
      transaction = res.data?.transactionByID;
      transactionForm.set({
        data: {
          amount: res.data.transactionByID.amount,
          merchant: res.data.transactionByID.merchant,
          notes: res.data.transactionByID.notes,
        },
        isDirty: false,
      });
    });
  });

  const onTransactionReceiptAdded = async ({transaction, files}: any) => {
    if(files?.length === 0) {
      return;
    }

    const formdata = new FormData();
    formdata.append(
      "tags",
      JSON.stringify({
        CorrelationID: transaction.id,
        Type: "TRANSACTION_RECEIPT",
        TransactionID: transactionID,
        Note: "",
      })
    );

    formdata.append(
      "file",
      files[0],
      files[0].name
    );

    const response = await fetch("/files", {
      method: "POST",
      body: formdata,
    });
    
    if (!response.ok) {
      throw new Error(`File upload failed: ${response.statusText}`);
    }
  }

  const updateTransaction = () => {
    const updateTransactionOp = mutationStore({
      client: $paymentsUrql,
      variables: {
        id: transactionID,
        updateDTO: {
          ...$transactionForm.data,
          amount: +$transactionForm.data.amount,
        },
      },
      query: gql`
        mutation TransactionUpdate(
          $id: String!
          $updateDTO: TransactionUpdateDTOInput!
        ) {
          transactionUpdate(id: $id, updateDTO: $updateDTO) {
            currency
            id
            amount
            transactionText
            tags
            merchant
          }
        }
      `,
    });

    updateTransactionOp.subscribe(() => {
      pageMode = "VIEW";
      transaction = {
        ...transaction,
        ...$transactionForm.data,
        amount: +$transactionForm.data.amount,
      };
    });
  };
</script>

{#if ($transactionsQuery === undefined || $transactionsQuery.fetching) && !transaction}
  <p>Loading...</p>
{:else if $transactionsQuery.error}
  <p>🙆‍♂️ Uh oh! Unable to fetch your bills!</p>
{:else if transaction}
  <div class="transaction">
    {#if pageMode === "VIEW"}
      <section class="title">
        <div class="amount">
          {#if transaction.amount !== null}
            <span class="value"
              >{new Intl.NumberFormat(undefined, {
                style: "currency",
                currency: "INR",
              }).format(transaction.amount)}</span
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
          {formatRelativeDate(new Date(transaction.paidAt))} • {new Intl.DateTimeFormat(
            "en-GB",
            {
              year: "2-digit",
              month: "long",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
              second: "2-digit",
            }
          ).format(new Date(transaction.paidAt))}
        </div>
      </section>
      <section class="content">
        <h1>Recipient</h1>
        <h2>{transaction.merchant ?? "Unknown"}</h2>

        <h1 class="subheader">Parsing Status</h1>
        <h2>
          {transaction.parseStatus}
        </h2>

        <h1 class="subheader">Associated Billing Account</h1>
        <h2>
          {transaction.bill.name}
        </h2>

        {#if transaction.transactionText !== ""}
          <h1 class="subheader">Original Transaction Detail</h1>
          <div class="transaction-detail">
            {transaction.transactionText}
          </div>
        {/if}

        <h1 class="subheader">Tags</h1>
        <div class="tags_description">
          This transaction doesn't have any tags.
        </div>
        <!-- {#if transaction.tags?.length === 0}
        {:else}
          <div class="tags">
            {#each transaction.tags as tag}
              <div class="tag">{tag}</div>
            {/each}
          </div>
        {/if} -->

        <h1 class="subheader">Notes</h1>
        <p class="note note__view">
          {transaction.notes || "This transaction doesn't have a note."}
        </p>
      </section>
      <!-- REGION: EDIT -->
    {:else}
      <section class="title title__edit">
        <input
          class="amount amount__edit"
          placeholder="Unknown Amount"
          bind:value={$transactionForm.data.amount}
        />
        <div class="transaction-detail">
          {formatRelativeDate(new Date(transaction.paidAt))} • {new Intl.DateTimeFormat(
            "en-GB",
            {
              year: "2-digit",
              month: "long",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
              second: "2-digit",
            }
          ).format(new Date(transaction.paidAt))}
        </div>
      </section>
      <section class="content">
        <h1>Recipient</h1>
        <input
          class="merchant merchant__edit"
          bind:value={$transactionForm.data.merchant}
        />

        <h1 class="subheader">Parsing Status</h1>
        <h2>
          {transaction.parseStatus}
        </h2>

        <h1 class="subheader">Associated Billing Account</h1>
        <h2>
          {transaction.bill.name}
        </h2>

        <h1 class="subheader">Original Transaction Detail</h1>
        <div class="transaction-detail">
          {transaction.transactionText}
        </div>

        <h1 class="subheader">Transaction receipt</h1>
        <div class="transaction-file-input">
          <FileUploader
            onFileAdded={({ files }) =>
              onTransactionReceiptAdded({ transaction, files })}
          />
        </div>

        <h1 class="subheader">Tags</h1>
        {#if transaction.tags?.length === 0}
          <div class="tags_description">
            This transaction doesn't have any tags.
          </div>
        {:else}
          <div class="tags">
            {#each transaction.tags as tag}
              <div class="tag">{tag}</div>
            {/each}
          </div>
        {/if}

        <h1 class="subheader">Notes</h1>
        <textarea
          class="note note__edit"
          bind:value={$transactionForm.data.notes}
          placeholder="This transaction doesn't have a note. Click here to add one."
        ></textarea>
        <button class="submit" on:click={() => updateTransaction()}>save</button
        >
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

  input {
    background-color: #e2e2e2;
    border: none;
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

  .transaction-file-input {
    margin: 1rem 0;
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

  .merchant {
    margin: 1rem 0;
  }
  .merchant__edit {
    font-size: 1rem;
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

  .tags_description {
    margin: 1rem 0;
    font-size: 0.75rem;
  }
</style>
