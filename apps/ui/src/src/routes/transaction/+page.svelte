<script lang="ts">
  import { paymentsUrql } from "$lib/stores/urql";
  import { faEdit } from "@fortawesome/free-solid-svg-icons";

  import { queryStore, gql, mutationStore } from "@urql/svelte";
  import type { OperationResultStore } from "@urql/svelte";
  import { onMount } from "svelte";
  import { formatRelativeDate } from "../../utils/format-relative-date";
  import { formStoreGenerator, type FormStore } from "$lib/stores/form";
  import IconButton from "$lib/icon-button.svelte";
  import type { Writable } from "svelte/store";
  import type { TransactionDTO as Transaction } from "$lib/types";
  import FileUploader from "../../lib/file-uploader.svelte";
  import { envStore } from "$lib/stores/env";
  import IdeaCard from "$lib/idea-card.svelte";
  import { nav } from "$lib/stores/nav";
  import type { TransactionTag } from "$lib/types";
	import { flip } from 'svelte/animate';
  import { send, receive } from '$lib/transitions/crossfade';

  let transactionID: string | null = null;
  let pageMode: "VIEW" | "EDIT" = "VIEW";
  let editCta = "Save";
  let transaction: any;
  let cancelCtaButton: HTMLButtonElement, saveCtaButton: HTMLButtonElement;
  let transactionsQuery: OperationResultStore;
  let transactionForm: Writable<FormStore<Transaction>> = formStoreGenerator("transactionById");

  let transactionReparseTriggered = false;
  let showTransactionTagsEdit = false;

  $: availableTags = ($transactionsQuery?.data?.transactionTags || []).filter((t: TransactionTag) => !$transactionForm?.data?.tags?.find(p => p.title === t.title)); 
  // $: {
  //   console.log('all', $transactionsQuery?.data?.transactionTags)
  //   console.log('availableTags', availableTags)
  //   console.log('for this transaction', $transactionForm?.data?.tags)
  // }

  const addFilesBaseUrlPrefix = ({ url }: { url: string }) => {
    return `${($envStore?.filesBaseUrl ? [$envStore.filesBaseUrl, url] : [url]).join("")}`;
  };

  onMount(() => {
    nav.update(prev => ({...prev, isOpen: true })); 
    transactionID = new URLSearchParams(window.location.search).get('id');
    transactionsQuery = queryStore({
      client: $paymentsUrql,
      variables: { id: transactionID },
      query: gql`
        query ($id: String!) {
          transactionTags {
            id title 
          }
          transactionByID(id: $id) {
            id
            amount
            merchant
            paidAt
            transactionText
            tags
            parseStatus
            notes
            receipts {
              id
              createdAt
              downloadPath
              updatedAt
              mimeType
              extension
            }
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

      const tags = res.data.transactionByID.tags.map((t: string) => res.data.transactionTags.find((p: TransactionTag) => p.title === t)) 

      transaction = { 
        ...res.data?.transactionByID,
        tags
      };

      transactionForm.set({
        data: {
          amount: res.data.transactionByID.amount,
          merchant: res.data.transactionByID.merchant,
          notes: res.data.transactionByID.notes,
          tags,
          receipts: res.data.transactionByID.receipts.map((receipt: any) => ({
            fileName: receipt.id,
            mimeType: receipt.mimeType,
            downloadPath: receipt.downloadPath,
          })),
          updatedReceipts: res.data.transactionByID.receipts.map(
            (receipt: any) => ({
              fileName: receipt.id,
              mimeType: receipt.mimeType,
              downloadPath: receipt.downloadPath,
            })
          ),
        },
        isDirty: false,
      });
    });
  });

  const onTransactionReceiptAdded = ({ file }: { file: File }) => {
    // console.log("file added:", file);
    transactionForm.update((form) => ({
      data: {
        amount: form.data.amount,
        merchant: form.data.merchant,
        notes: form.data.notes,
        receipts: form.data.receipts,
        updatedReceipts: [...form.data.updatedReceipts, file],
      },
      isDirty: false,
    }));
  };

  const onTransactionReceiptRemoved = ({ file }: { file: File }) => {
    transactionForm.update((form) => ({
      data: {
        amount: form.data.amount,
        merchant: form.data.merchant,
        notes: form.data.notes,
        receipts: form.data.receipts,
        updatedReceipts: (form.data.updatedReceipts || []).filter(
          (r) => r !== file
        ),
      },
      isDirty: false,
    }));
  };

  const triggerReparse = async () => {
    try {
      transactionReparseTriggered = true;
      await $paymentsUrql
        .mutation(
          gql`
            mutation TransactionUpdate(
              $id: String!
              $updateDTO: TransactionUpdateDTOInput!
            ) {
              transactionUpdate(id: $id, updateDTO: $updateDTO) {
                id
                parseStatus
              }
            }
          `,
          {
            id: transactionID,
            updateDTO: {
              parseStatus: "NotStarted",
            },
          }
        )
        .toPromise();
    } catch {
      // TODO: Use common drawer to show transaction update error
    }
  };

  const updateTransaction = async () => {
    editCta = "Saving...";

    cancelCtaButton.disabled = true;
    saveCtaButton.disabled = true;

    const existingReceipts = $transactionForm.data.receipts;
    const newReceipts = $transactionForm.data.updatedReceipts;

    // New Files to upload are the ones which don't have a downloadPath
    const receiptsToAdd = newReceipts.filter((p) => !p?.downloadPath);

    // Existing files to delete are the ones which are not present in the new files
    const receiptsToDelete = existingReceipts.filter(
      (p) =>
        newReceipts.findIndex((x) => x?.downloadPath === p?.downloadPath) === -1
    );

    // console.log({existingReceipts: $transactionForm.data.receipts})
    // console.log({newReceipts: $transactionForm.data.updatedReceipts})
    // console.log({receiptsToAdd})
    // console.log({receiptsToDelete})

    const deleteReceiptPromises = receiptsToDelete.map((receipt: any) => {
      return fetch(`${addFilesBaseUrlPrefix({ url: receipt.downloadPath })}`, {
        method: "DELETE",
      }).then((response) => {
        if (!response.ok) {
          throw new Error(`File deletion failed: ${response.statusText}`);
        }
        return receipt;
      });
    });

    // Upload new files
    const uploadPromises = receiptsToAdd.map((file: File) => {
      const formdata = new FormData();
      formdata.append(
        "tags",
        JSON.stringify({
          CorrelationID: transactionID,
          Type: "TRANSACTION_RECEIPT",
          TransactionID: transactionID,
          Note: "",
        })
      );
      formdata.append("file", file, file.name);

      return fetch("/files/files", {
        method: "POST",
        body: formdata,
      }).then((response) => {
        if (!response.ok) {
          throw new Error(`File upload failed: ${response.statusText}`);
        }
        return Promise.resolve();
      });
    });

    await Promise.all([...uploadPromises, ...deleteReceiptPromises]);

    const updateTransactionOp = mutationStore({
      client: $paymentsUrql,
      variables: {
        id: transactionID,
        updateDTO: {
          ...$transactionForm.data,
          amount: +$transactionForm.data.amount,
          receipts: undefined,
          updatedReceipts: undefined,
          tags: undefined
        },
        tags: $transactionForm.data.tags.map((t: TransactionTag) => t.title).join(",")
      },
      query: gql`
        mutation TransactionUpdate(
          $id: String!
          $updateDTO: TransactionUpdateDTOInput!
          $tags: String!
        ) {
          transactionReceiptsSync(input: { transactionID: $id }) {
            id
            createdAt
            downloadPath
            updatedAt
            mimeType
            extension
          }

          transactionUpdate(id: $id, updateDTO: $updateDTO) {
            currency
            id
            amount
            transactionText
            merchant
          } 

          setTransactionTags(id: $id, tags: $tags) {
            id
            tags
          }
        }
      `,
    });

    updateTransactionOp.subscribe(({ fetching, data }) => {
      if (fetching) {
        return;
      }

      if (saveCtaButton) saveCtaButton.disabled = false;
      if (cancelCtaButton) cancelCtaButton.disabled = false;
      editCta = "Save";
      pageMode = "VIEW";

      transaction = {
        ...transaction,
        ...$transactionForm.data,
        amount: +$transactionForm.data.amount,
      };
    });
  };
</script>

<section class="container">
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
              scale={1.5}
              style={'padding: 0'}
              on:click={() => { pageMode = "EDIT"; }}
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

          <h1 class="subheader parse-status">Parsing Status</h1>
          <div class="parse-status">
            <h2>
              {transaction.parseStatus}
            </h2>
            {#if transaction.parseStatus !== "NotStarted"}
              <button on:click={triggerReparse}>Reparse</button>
            {/if}
          </div>

          <IdeaCard
            idea={transaction.parseStatus !== "NotStarted"
              ? "NEW! Use the reparse option to trigger a new extraction of the transaction details using GenAI."
              : "Details for this transaction will be available soon."}
          />

          <h1 class="subheader">Associated Billing Account</h1>
          <h2>
            {transaction.bill.name}
        </h2>

          {#if transaction.transactionText !== ""}
            <h1 class="subheader">Original Transaction Detail</h1>
            <div class="transaction-detail">
              <p>{transaction.transactionText}</p>
            </div>
          {/if}

          {#if transaction.receipts}
            <h1 class="subheader">Receipts</h1>
            <div class="receipts">
              {#if transaction.receipts.length === 0}
                <p>
                  This transaction doesn't have any receipts. Edit transaction
                  to add receipts.
                </p>
              {:else}
                <div class="transaction-file-input">
                  <FileUploader
                    editable={false}
                    fileUrlTransformer={addFilesBaseUrlPrefix}
                    files={transaction.receipts.map((receipt: any) => ({
                      fileName: receipt.fileName,
                      mimeType: receipt.mimeType,
                      downloadPath: receipt.downloadPath,
                    }))}
                  />
                </div>
              {/if}
            </div>
          {/if}

          <h1 class="subheader">Tags</h1>
          {#if transaction.tags?.length === 0}
           <div class="tags_description">
          This transaction doesn't have any tags.
        </div>
          {:else}
            <div class="tags">
              {#each transaction.tags as tag}
                <div class="tag">{tag.title}</div>
              {/each}
            </div>
          {/if}

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
            <p>{transaction.transactionText}</p>
          </div>

          <h1 class="subheader">Transaction receipt</h1>
          <div class="transaction-file-input">
            <FileUploader
              editable={true}
              files={$transactionForm.data.updatedReceipts}
              onFileAdded={onTransactionReceiptAdded}
              onFileRemoved={onTransactionReceiptRemoved}
              fileUrlTransformer={addFilesBaseUrlPrefix}
            />
          </div>

        <section class="tags__edit">
              <h1 class="subheader tags">Edit Tags</h1>
        {#if $transactionForm.data.tags?.length === 0}
          <div class="tags_description">
            This transaction doesn't have any tags.
          </div>
        {:else}
              <h2 class="subheader tags">Added Tags</h2>
              <div class="tags">
                {#each $transactionForm?.data?.tags as tag (tag.id)}
                  <button in:receive={{ key: tag.id }}
                    			out:send={{ key: tag.id }}
			                    animate:flip={{ duration: 200 }}
                    on:click={() => { transactionForm.update(prev => ({data: {...prev.data, tags: prev.data.tags.filter(p => p.title !== tag.title) }})); }}>
                    <div class="tag">{tag.title}</div>
                  </button>
                {/each}
              </div>

        {/if} 

              <h2 class="subheader tags">Available Tags</h2>
              <div class="tags">
                {#each availableTags as tag (tag.id)}
                <button in:receive={{ key: tag.id }}
                    		out:send={{ key: tag.id }}
			                  animate:flip={{ duration: 200 }}
                        on:click={(() => { transactionForm.update(prev => ({data: {...prev.data, tags: [...prev.data.tags, tag]}})); })}>
                  <div class="tag">{tag.title}</div>
                </button>
                {/each}
              </div>
        </section>

          <h1 class="subheader">Notes</h1>
          <textarea
            class="note note__edit"
            bind:value={$transactionForm.data.notes}
            placeholder="This transaction doesn't have a note. Click here to add one."
          ></textarea>
          <button
            class="submit"
            bind:this={saveCtaButton}
            on:click={() => updateTransaction()}>{editCta}</button
          >
          <button
            class="submit cta--cancel"
            bind:this={cancelCtaButton}
            on:click={() => (pageMode = "VIEW")}>Cancel</button
          >
        </section>
      {/if}
    </div>
  {/if}
</section>

<style>
  .container {
    padding: 1rem;
  }

  .submit {
    margin-top: 1rem;
  }
  .cta--cancel {
    background-color: var(--secondary-bg-color);
  }

  .subheader {
    margin-top: 0.75rem;
  }

  .parse-status {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    margin-bottom: 0.75rem;
  }

  input {
    background-color: #0000007f;
    border: none;
    border-radius: .5rem;
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

  .content {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }

  .tags--title__edit {
    display: flex;
    align-items: center;
  }

  .tags__edit button {
    padding: 0; text-transform: unset;
  }

  h1.tags {
    display: inline;
  }

  .transaction {
    height: 100%;
    display: flex;
    flex-direction: column;
  }

  .transaction-detail {
    font-size: 0.75rem;
    font-weight: 400;
    /* margin: 1rem auto; */
  }

  .transaction-file-input {
    margin: 1rem 0;
  }

  p {
    margin: 1rem 0;
  }

  .title {
    margin-bottom: 1rem;
  }

  h1 {
    margin: 0;
  }

  h2 {
    font-size: 1rem;
  }

  .amount {
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

  .receipts > p {
    margin: 1rem 0;
  }

  .transaction-receipts-list {
    margin: 0 1rem;
    padding: 0;
  }

</style>
