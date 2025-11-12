<script lang="ts">
  import Timeline from "$lib/timeline.svelte";
  import RecentTransactions from "$lib/recent-transactions.svelte";
  import { goto } from "$app/navigation";
  import Nav from "$lib/nav.svelte";
  import { auth } from "$lib/stores/auth";
  import { liteDb } from "$lib/stores/lite-indexed-db"
  import { onMount } from "svelte";
  import { LiteBillService } from "../../../utils/lite/lite-bills.service";
  import { LiteBillStatementsService } from "../../../utils/lite/lite-bill-statements.service";
  import { LiteTransactionsService } from "../../../utils/lite/lite-transactions.service";
  import { writable } from "svelte/store";
  import type { AddBillStatementDTO } from "$lib/types";

  let billsService: LiteBillService;
  let billStatementsService: LiteBillStatementsService;
  let transactionsService: LiteTransactionsService;

  onMount(() => {
    billsService = new LiteBillService($liteDb);
    billStatementsService = new LiteBillStatementsService($liteDb);
    transactionsService = new LiteTransactionsService($liteDb);
  });

  $: billsQuery = billsService?.queryBills();
  $: billStatementsQuery =
    !$billsQuery?.fetching && !$billsQuery?.error
      ? billStatementsService?.queryBillStatementsByBillIds(
          $billsQuery.data?.map((bill) => bill.id) ?? []
        )
      : null;

  const currentMonth = new Date().getUTCMonth() + 1;
  const currentYear = new Date().getUTCFullYear();

  const billStatsQuery = writable({
    fetching: false,
    data: { billStats: { stats: [] } },
    error: null,
  });

  //  queryStore({
  //   client: $billsUrql,
  //   query: gql`
  //       {
  //       billStats(year: ${currentYear.toString()}, month: ${currentMonth.toString()}) {
  //           startDate,
  //           endDate,
  //           stats {
  //               type
  //               billIds
  //               dateRanges {
  //                   start
  //                   end
  //               }
  //           }
  //       }
  //   }
  //   `,
  // });

  $: transactionsQuery =
    transactionsService?.queryTransactionsForCurrentMonth();

  // queryStore({
  //   client: $paymentsUrql,
  //   variables: { year: currentYear, month: currentMonth },
  //   query: gql`
  //     query ($year: Int!, $month: Int!) {
  //       transactions: transactionsByYearAndMonth(
  //         year: $year
  //         month: $month
  //         first: 900
  //       ) {
  //         nodes {
  //           id
  //           amount
  //           merchant
  //           paidAt
  //           tags
  //         }
  //         pageInfo {
  //           hasNextPage
  //           startCursor
  //           endCursor
  //         }
  //       }
  //     }
  //   `,
  // });

  const onRecordingPayment = (billStatementDTO: AddBillStatementDTO) => {
    return billStatementsService.addBillStatement(billStatementDTO);
    // $paymentsUrql
    //   .mutation(
    //     gql`
    //       mutation AddBillStatement($dto: AddOrUpdateBillStatementDTOInput!) {
    //         addOrUpdateBillStatement(dto: $dto) {
    //           id
    //           startDate
    //           endDate
    //           isFullyPaid
    //           amount
    //           payments {
    //             id
    //             __typename
    //           }
    //         }
    //       }
    //     `,
    //     {
    //       dto: {
    //         notes: "",
    //         amount,
    //         isFullyPaid,
    //         bill: { id: +bill.id },
    //         startDate: cycleFromDate,
    //         endDate: cycleToDate,
    //         edges: { paymentIds: [] },
    //       },
    //     }
    //   )
    //   .toPromise()
    //   .then((res) => {
    //     if (res.error) {
    //       console.error("Error recording payment:", res.error);
    //       throw new Error("Failed to record payment");
    //     }
    //   });
  };
</script>

<section class="timeline-page">
  {#if $billsQuery?.fetching || $billStatsQuery?.fetching || $transactionsQuery?.fetching}
    <p>Loading...</p>
  {:else if $billsQuery?.error || $billStatsQuery?.error || $transactionsQuery?.error}
    <p>🙆 Uh oh! Unable to fetch your bills!</p>
  {:else}
    <Timeline
      items={$billsQuery?.data || []}
      stats={$billStatsQuery?.data?.billStats || { stats: [] }}
      transactions={$transactionsQuery?.data || []}
      billingStatements={$billStatementsQuery?.data}
      {onRecordingPayment}
    />
  {/if}
</section>

<style>
  .timeline-page {
    margin: 1rem;
  }

  p {
    margin: 0;
  }
</style>
