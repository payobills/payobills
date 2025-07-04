<script lang="ts">
  import { paymentsUrql } from "$lib/stores/urql";
  import { faEdit } from "@fortawesome/free-solid-svg-icons";

  import { queryStore, gql, mutationStore } from "@urql/svelte";
  import type { OperationResultStore } from "@urql/svelte";
  import { onMount } from "svelte";
  import { formatRelativeDate } from "../../../utils/format-relative-date";
  import { formStoreGenerator, type FormStore } from "$lib/stores/form";
  import IconButton from "$lib/icon-button.svelte";
  import type { Writable } from "svelte/store";
  import type { Transaction } from "$lib/types/transaction";
  import FileUploader from "../../../lib/file-uploader.svelte";
  import { envStore } from "$lib/stores/env";

  let transactionID: string | null = null;
  let pageMode: "VIEW" | "EDIT" = "VIEW";
  let editCta = "Save";
  let transaction: any;
  let cancelCtaButton: HTMLButtonElement, saveCtaButton: HTMLButtonElement;
  let transactionsQuery: OperationResultStore;
  let transactionForm: Writable<FormStore<Transaction>> =
    formStoreGenerator("transactionById");

  const addFilesBaseUrlPrefix = ({ url }: { url: string }) => {
    return `${($envStore?.filesBaseUrl ? [$envStore.filesBaseUrl, url] : [url]).join("")}`;
  };

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
      transaction = res.data?.transactionByID;
      transactionForm.set({
        data: {
          amount: res.data.transactionByID.amount,
          merchant: res.data.transactionByID.merchant,
          notes: res.data.transactionByID.notes,
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
    // console.log("removing file:", file);
    transactionForm.update((form) => ({
      data: {
        amount: form.data.amount,
        merchant: form.data.merchant,
        notes: form.data.notes,
        receipts: form.data.receipts,
        // form.data.receipts.map((receipt: any) => ({
        //   fileName: receipt.id,
        //   mimeType: receipt.mimeType,
        //   downloadPath: receipt.downloadPath,
        // })),
        updatedReceipts: (form.data.updatedReceipts || []).filter(
          (r) => r !== file
        ),
      },
      isDirty: false,
    }));
  };

  // const onTransactionReceiptAdded = async ({ transaction, files }: any) => {
  //   if (files?.length === 0) {
  //     return;
  //   }

  //   const formdata = new FormData();
  //   formdata.append(
  //     "tags",
  //     JSON.stringify({
  //       CorrelationID: transaction.id,
  //       Type: "TRANSACTION_RECEIPT",
  //       TransactionID: transactionID,
  //       Note: "",
  //     })
  //   );

  //   formdata.append("file", files[0], files[0].name);

  //   const response = await fetch("/files", {
  //     method: "POST",
  //     body: formdata,
  //   });

  //   if (!response.ok) {
  //     throw new Error(`File upload failed: ${response.statusText}`);
  //   }
  // };

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
        },
      },
      query: gql`
        mutation TransactionUpdate(
          $id: String!
          $updateDTO: TransactionUpdateDTOInput!
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
            tags
            merchant
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
            on:click={() => {
              pageMode = "EDIT";

              transactionForm.set({
                data: {
                  amount: $transactionsQuery.data.transactionByID.amount,
                  merchant: $transactionsQuery.data.transactionByID.merchant,
                  notes: $transactionsQuery.data.transactionByID.notes,
                  receipts:
                    $transactionsQuery.data.transactionByID.receipts.map(
                      (receipt: any) => ({
                        fileName: receipt.id,
                        mimeType: receipt.mimeType,
                        downloadPath: receipt.downloadPath,
                      })
                    ),
                  updatedReceipts:
                    $transactionsQuery.data.transactionByID.receipts.map(
                      (receipt: any) => ({
                        fileName: receipt.id,
                        mimeType: receipt.mimeType,
                        downloadPath: receipt.downloadPath,
                      })
                    ),
                },
                isDirty: false,
              });
            }}
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
            <p>{transaction.transactionText}</p>
          </div>
        {/if}

        {#if transaction.receipts}
          <h1 class="subheader">Receipts</h1>
          <div class="receipts">
            {#if transaction.receipts.length === 0}
              <p>
                This transaction doesn't have any receipts. Edit transaction to
                add receipts.
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
        <button
          class="submit"
          bind:this={saveCtaButton}
          on:click={() => updateTransaction()}>{editCta}</button
        >
        <button
          class="submit cta--cancel"
          bind:this={cancelCtaButton}
          on:click={() => (pageMode = "VIEW")}>cancel</button
        >
      </section>
    {/if}
  </div>
{/if}

<style>
  .submit {
    margin-top: 1rem;
  }
  .cta--cancel {
    background-color: var(--secondary-bg-color);
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
    /* margin: 1rem auto; */
  }

  .transaction-file-input {
    margin: 1rem 0;
  }

  p {
    margin: 1rem 0;
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

  .receipts > p {
    margin: 1rem 0;
  }

  .transaction-receipts-list {
    margin: 0 1rem;
    padding: 0;
  }
</style>
