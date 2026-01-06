<script lang="ts">
import { onMount } from "svelte";
import { CONSTANTS } from "../../../constants";
import { nav } from "../../../lib/stores/nav";

$: billsService = $liteServices?.billsService;
$: billStatementsService = $liteServices?.billStatementsService;
$: transactionsService = $liteServices?.transactionsService;

onMount(() => {
	nav.update((prev) => ({
		...prev,
		title: CONSTANTS.PAYOBILLS_LITE,
		isOpen: true,
	}));
});

$: billsQuery = billsService?.queryBills();
$: billStatementsQuery =
	!$billsQuery?.fetching && !$billsQuery?.error
		? billStatementsService?.queryBillStatementsByBillIds(
				$billsQuery.data?.map((bill) => bill.id) ?? [],
			)
		: null;

const currentMonth = new Date().getUTCMonth() + 1;
const currentYear = new Date().getUTCFullYear();

$: billStatsQuery = billStatementsService?.queryBillStatementsByBillIds(
	$billsQuery?.data?.map((bill) => bill.id) ?? [],
);

$: transactionsQuery = transactionsService?.queryTransactionsForMonthAndYear({
	month: currentMonth,
	year: currentYear,
});

const _onRecordingPayment = ({
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
