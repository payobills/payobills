<script lang="ts">
  import Timeline from "$lib/timeline.svelte";
  import RecentTransactions from "$lib/recent-transactions.svelte";
  import { goto } from "$app/navigation";
  import Nav from "$lib/nav.svelte";
  import { auth } from "$lib/stores/auth";
  import { liteDb } from "$lib/stores/lite-indexed-db";
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

  $: billStatsQuery = billStatementsService?.queryBillStatementsByBillIds(
    $billsQuery?.data?.map((bill) => bill.id) ?? []
  );

  $: transactionsQuery =
    transactionsService?.queryTransactionsForCurrentMonth();

  const onRecordingPayment = ({
    bill,
    amount,
    isFullyPaid,
    cycleFromDate,
    cycleToDate,
  }) => {
    return billStatementsService.addBillStatement({
      notes: "",
      amount,
      isFullyPaid,
      bill: { id: bill.id },
      startDate: cycleFromDate,
      endDate: cycleToDate,
      edges: { paymentIds: [] },
    });
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
      stats={{ stats: $billStatsQuery?.data || [] }}
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
