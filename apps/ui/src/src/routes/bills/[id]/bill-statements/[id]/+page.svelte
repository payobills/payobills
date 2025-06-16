<script lang="ts">
  import { queryStore, gql } from "@urql/svelte";
  import { billsUrql, paymentsUrql } from "$lib/stores/urql";
  import { page } from "$app/stores";
  import PaymentTimelinePill from "$lib/payment-timeline-pill.svelte";
  import Nav from "$lib/nav.svelte";
  import Card from "$lib/card.svelte";
  import { onMount } from "svelte";
  import BillUploadStatement from "$lib/bill-upload-statement.svelte";
  import BillStatements from "$lib/bills/bill-statements.svelte";
  import { json } from "@sveltejs/kit";
  import FileUploader from "$lib/file-uploader.svelte";
  import { envStore } from "$lib/stores/env";
  import RecentTransactions from "$lib/recent-transactions.svelte";

  let billId: any;
  let billStatementId: any;
  let billStatementsQuery: any;
  let refreshKey: number = Date.now();

  onMount(() => {
    let path = window.location.pathname;
    billId = path.split("/")[2];
    billStatementId = path.split("/")[4];
  });

  $: billStatementsQuery = queryStore({
    client: $billsUrql,
    query: gql`
      query billById($billId: String!) {
        billStatements(billId: $billId) {
          id
          startDate
          endDate
          notes
          statement {
            id
            ocrID
            createdAt
            updatedAt
          }
        }
        bill: billById(id: $billId) {
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

  $: currentBillStatement = $billStatementsQuery?.data?.billStatements
    ? $billStatementsQuery.data.billStatements.find(
        (billStatement: any) => billStatement.id === billStatementId
      )
    : undefined;

  $: transactionsFromOCRQuery = currentBillStatement?.statement?.ocrID
    ? queryStore({
        client: $paymentsUrql,
        query: gql`
          query GetTransactionsFromOCR($ocrID: String!) {
            transactions(filters: { ocrId: $ocrID }) {
              nodes {
                id
                amount
                merchant
                paidAt
                tags
              }
              pageInfo {
                hasNextPage
                startCursor
                endCursor
              }
            }
          }
        `,
        variables: { ocrID: currentBillStatement?.statement?.ocrID },
        pause: currentBillStatement?.statement?.ocrID === undefined,
      })
    : null;

  const addFilesBaseUrlPrefix = ({ url }: { url: string }) => {
    return `${($envStore?.filesBaseUrl ? [$envStore.filesBaseUrl, url] : [url]).join("")}`;
  };
</script>

<Card>
  <div class="content">
    {#if $billStatementsQuery.fetching}
      <p>Loading...</p>
    {:else if $billStatementsQuery.error}
      <p>🙆‍♂️ Uh oh! Unable to fetch your bill!</p>
    {:else}
      <h1>
        <a href={`/bills/${$billStatementsQuery.data.bill.id}`}
          >{$billStatementsQuery.data.bill.name}</a
        >
        / {!currentBillStatement.startDate || !currentBillStatement.endDate
          ? "Unknown billing period"
          : `${currentBillStatement.startDate} to ${currentBillStatement.endDate}`}
      </h1>

      {#if currentBillStatement.statement}
        <p>This bill statement has an associated statement file</p>
        <FileUploader
          editable={false}
          files={[currentBillStatement.statement]}
          fileUrlTransformer={addFilesBaseUrlPrefix}
        />

        {#if $transactionsFromOCRQuery?.fetching}
          <p>Loading...</p>
        {:else if $transactionsFromOCRQuery?.error}
          <p>🙆‍♂️ Uh oh! Unable to fetch your bill!</p>
        {:else if (($transactionsFromOCRQuery?.data?.transactions?.nodes || []).length === 0)}
          <p>🙆‍♂️ Uh oh! Looks like the statement file couldn't be parsed for transactions!</p>
          {:else}
          <RecentTransactions
            initialShowCount={Infinity}
            title="Transactions from this statement"
            totalSpend={$transactionsFromOCRQuery?.data?.transactions?.nodes.reduce(
              (acc: number, curr: any) => acc + curr.amount,
              0
            )}
            showViewAllCTA={false}
            transactions={$transactionsFromOCRQuery?.data?.transactions
              ?.nodes || []}
          />
        {/if}
      {:else}
        <p>This bill statement doesn't have an associated statement file.</p>
      {/if}
    {/if}
  </div>
</Card>

<style>
  h1,
  a {
    color: var(--primary-color);
    font-size: 1.2rem;
    font-weight: 600;
  }

  p {
    font-size: 0.8rem;
  }
  .content {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }
  .actions {
    display: flex;
    flex-grow: 1;
    justify-content: flex-end;
    flex-direction: column;
  }

  .actions > button {
    margin: 0.25rem 0;
    width: 100%;
  }

  .markPaid {
    align-self: flex-end;
  }

  .payment {
    display: flex;
  }

  .amount,
  .amount--unknown {
    flex-grow: 1;
  }
</style>
