<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import BillPayment from "$lib/bills/bill-payment.svelte";

  const baseBill = {
    id: "bill-1",
    name: "HDFC Credit Card",
    billingDate: 5,
    payByDate: 25,
    isEnabled: true,
    primaryType: "Credit Card",
    createdAt: new Date(),
    updatedAt: new Date(),
    payments: [],
  };

  const paidStatement = {
    id: "stmt-1",
    notes: "",
    amount: 12500,
    bill: { id: "bill-1" },
    startDate: "2026-06-05",
    endDate: "2026-07-05",
    isFullyPaid: true,
    edges: { paymentIds: [] },
  };

  const unpaidStatement = {
    id: "stmt-2",
    notes: "",
    amount: 0,
    bill: { id: "bill-1" },
    startDate: "2026-06-05",
    endDate: "2026-07-05",
    isFullyPaid: false,
    edges: { paymentIds: [] },
  };

  const noop = () => {};

  const { Story } = defineMeta({
    title: "Bills/BillPayment",
    component: BillPayment,
    tags: ["autodocs"],
  });
</script>

<Story name="Due Soon">
  {#snippet children()}
    <div style="max-width: 420px;">
      <BillPayment
        bill={baseBill}
        billingStatements={[unpaidStatement]}
        onRecordingPayment={noop}
        onCurrentBillStatementDoesNotExist={noop}
        onTransactionSearch={noop}
        onLoadMoreTransactions={noop}
      />
    </div>
  {/snippet}
</Story>

<Story name="Fully Paid">
  {#snippet children()}
    <div style="max-width: 420px;">
      <BillPayment
        bill={baseBill}
        billingStatements={[paidStatement]}
        onRecordingPayment={noop}
        onCurrentBillStatementDoesNotExist={noop}
        onTransactionSearch={noop}
        onLoadMoreTransactions={noop}
      />
    </div>
  {/snippet}
</Story>

<Story name="Disabled Bill">
  {#snippet children()}
    <div style="max-width: 420px;">
      <BillPayment
        bill={{ ...baseBill, isEnabled: false, name: "Old Insurance" }}
        billingStatements={[]}
        onRecordingPayment={noop}
        onCurrentBillStatementDoesNotExist={noop}
        onTransactionSearch={noop}
        onLoadMoreTransactions={noop}
      />
    </div>
  {/snippet}
</Story>

<Story name="Overdue">
  {#snippet children()}
    <div style="max-width: 420px;">
      <BillPayment
        bill={{ ...baseBill, name: "Internet Bill", primaryType: "Utility", payByDate: 10 }}
        billingStatements={[unpaidStatement]}
        onRecordingPayment={noop}
        onCurrentBillStatementDoesNotExist={noop}
        onTransactionSearch={noop}
        onLoadMoreTransactions={noop}
      />
    </div>
  {/snippet}
</Story>

<Story name="No Record Button">
  {#snippet children()}
    <div style="max-width: 420px;">
      <BillPayment
        bill={baseBill}
        billingStatements={[unpaidStatement]}
        showRecordPaymentButton={false}
        onRecordingPayment={noop}
        onCurrentBillStatementDoesNotExist={noop}
        onTransactionSearch={noop}
        onLoadMoreTransactions={noop}
      />
    </div>
  {/snippet}
</Story>
