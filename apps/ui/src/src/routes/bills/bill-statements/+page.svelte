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
  import { nav } from "$lib/stores/nav";
  import RecentTransactions from "$lib/recent-transactions.svelte";
  import { currencyFormatter } from "../../../utils/currency-formatter.util";
    import UiDrawer from "$lib/ui-drawer.svelte";
    import RecordPaymentForm from "$lib/record-payment-form.svelte";
    import { writable, type Writable } from "svelte/store";
    import type { BillStatementDTO, Query, TransactionDTO } from "$lib/types";
    import { ProTransactionsService } from "../../../utils/pro/pro-transactions.service";

  let billId: any;
  let billStatementId: any;
  let billStatementsQuery: any;
  let refreshKey: number = Date.now();
  
  let showRecordPayment = false;

  const onRecordingPayment = ({
    amount,
    bill,
    cycleFromDate,
    cycleToDate,
    isFullyPaid,
    transactions
  }) => {
    return $paymentsUrql
      .mutation(
        gql`
          mutation AddBillStatement($dto: AddOrUpdateBillStatementDTOInput!) {
            addOrUpdateBillStatement(dto: $dto) {
              id
              startDate
              endDate
              isFullyPaid
              amount
              payments {
                id
                __typename
              }
            }
          }
        `,
        {
          dto: {
            id: billStatementId,
            notes: "",
            amount,
            isFullyPaid,
            bill: { id: +bill.id },
            startDate: cycleFromDate,
            endDate: cycleToDate,
            edges: { paymentIds: (transactions ?? []).map((transaction: TransactionDTO) => transaction.id) },
          },
        }
      )
      .toPromise()
      .then((res) => {
        if (res.error) {
          console.error("Error recording payment:", res.error);
          throw new Error("Failed to record payment");
        }
      });
  }



  let matchingTransactionsQuery: Writable<Query<TransactionDTO[]>> = writable({
    fetching: false,
    data: [],
    error: null
  });
  
  const onTransactionSearch = (transactionSearchTerm: string) => {
    if (!$paymentsUrql || !transactionSearchTerm) matchingTransactionsQuery

    const transactionService =  new ProTransactionsService($paymentsUrql)
    transactionService.queryTransactionsWithSearchTerm(matchingTransactionsQuery, transactionSearchTerm)

    return matchingTransactionsQuery
  }

  onMount(() => {
    nav.set({ isOpen: true })
    let urlParams = window.location.search;
    billId = new URLSearchParams(urlParams).get('bill-id')
    billStatementId = new URLSearchParams(urlParams).get('bill-statement-id')
  });

  $: billStatementsQuery = queryStore({
    client: $billsUrql,
    query: gql`
      query billById($billId: String!) {
        billStatements(billId: $billId) {
          id
          payments {
            id
            amount
            paidAt
          }
          amount
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

  // $: {
  //   console.log('all statements', $billStatementsQuery?.data?.billStatements)
  //   console.log('curr',  $billStatementsQuery?.data?.billStatements?.find((billStatement: any) => billStatement.id === billStatementId))
  // }

  $: currentBillStatement = $billStatementsQuery?.data?.billStatements
    ? $billStatementsQuery.data.billStatements.filter(
        (billStatement: any) => billStatement.id === billStatementId
      )?.[0]: undefined

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

      {#if currentBillStatement.amount}
        <h2>Bill amount: {currencyFormatter(currentBillStatement.amount)}</h2>
      {/if}

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
        {:else if ($transactionsFromOCRQuery?.data?.transactions?.nodes || []).length === 0}
          <p>
            🙆‍♂️ Uh oh! Looks like the statement file couldn't be parsed for
            transactions!
          </p>
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

      {#if currentBillStatement && currentBillStatement.payments.reduce((acc: number, payment: any) => acc + payment.amount, 0)}
        <!-- <div class="card-item"> -->

        {#if currentBillStatement.payments.length > 0}
          <h2>Payments made this cycle</h2>
          <ul>
            {#each currentBillStatement.payments as payment}
              <li>
                ₹ {payment.amount} on {new Intl.DateTimeFormat("en-GB", {
                  year: "2-digit",
                  month: "long",
                  day: "2-digit",
                  hour: "2-digit",
                  minute: "2-digit",
                  second: "2-digit",
                }).format(new Date(payment.paidAt))}
              </li>
            {/each}
          </ul>
        {/if}

        <p>
          Total Payments made this cycle: ₹ <strong>
            {currentBillStatement.payments.reduce(
              (acc: number, payment: any) => acc + payment.amount,
              0
            )} /-
          </strong>
        </p>

      {/if}
    {/if}

      {#if showRecordPayment}
        <UiDrawer onClose={() => {(showRecordPayment) = false}}>
          <RecordPaymentForm
            bill={$billStatementsQuery.data.bill} 
            billStatements={[currentBillStatement]}
            selectedStatement={currentBillStatement}
            lockBillStatementCycle={true}
            {onRecordingPayment}
            {onTransactionSearch}
          />
        </UiDrawer>
      {/if}

      <div class="actions">
        {#if !showRecordPayment}
          <button on:click={() => (showRecordPayment = true)}
            >Record Payment</button
          >
        {/if}
      </div>
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
</style>
