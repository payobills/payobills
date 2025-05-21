<script lang="ts">
  import { queryStore, gql } from "@urql/svelte";
  import { billsUrql } from "$lib/stores/urql";
  import { page } from "$app/stores";
  import PaymentTimelinePill from "$lib/payment-timeline-pill.svelte";
  import Nav from "$lib/nav.svelte";
  import Card from "$lib/card.svelte";
  import { onMount } from "svelte";
  import BillUploadStatement from "$lib/bill-upload-statement.svelte";
  import BillStatements from "$lib/bills/bill-statements.svelte";
  import { json } from "@sveltejs/kit";

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
</script>

<Card>
  <div class="content">
    <!-- <h2>bill statement</h2> -->

    <!-- <p>bill {billId}</p>
    <p>billStatementId {billStatementId}</p> -->

    {#if $billStatementsQuery.fetching}
      <p>Loading...</p>
    {:else if $billStatementsQuery.error}
      <p>🙆‍♂️ Uh oh! Unable to fetch your bill!</p>
    {:else}
      <h1>
        {$billStatementsQuery.data.bill.name} / {!currentBillStatement.startDate ||
        !currentBillStatement.endDate
          ? "Unknown billing period"
          : `${currentBillStatement.startDate} to ${currentBillStatement.endDate}`}
      </h1>
      <pre> {JSON.stringify($billStatementsQuery.data.billStatements, undefined, 1)}</pre>
    {/if}
  </div>
</Card>

<style>
  h1 {
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
