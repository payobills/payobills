<script lang="ts">
  import BillPayment from "./bills/bill-payment.svelte";
  import Button from "./button.svelte";

  export let bill: any;
  export let onRecordingPayment;

  let fullyPaid = true;
  let amount: number;

  let cycleFromDate: string, cycleToDate: string;
</script>

<div class="container">
  <div class="form-container">
    <h2>Record a payment</h2>
    <label for="record-bill-payment-bill">Bill</label>
    <BillPayment
      {bill}
      billingStatements={[]}
      showRecordPaymentButton={false}
      onRecordingPayment={() => {}}
      bind:currentCycleFromDate={cycleFromDate}
      bind:currentCycleToDate={cycleToDate}
    />

    <label for="record-bill-payment-amount">Amount</label>
    <input
      id="record-bill-payment-amount"
      type="number"
      placeholder="Enter amount paid"
      bind:value={amount}
    />

    <div class="checkbox-group">
      <label for="record-bill-payment-fullypaid">Fully Paid</label>
      <input
        type="checkbox"
        bind:checked={fullyPaid}
        name="record-bill-payment-fullypaid"
        id="record-bill-payment-fullypaid"
      />
      <!-- </div> -->
    </div>
  </div>
  <Button
    onclick={() =>
      onRecordingPayment({ amount, bill, cycleFromDate, cycleToDate })}
    >Record Payment</Button
  >
</div>

<style>

  .container {
    height: 100%;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 1rem;
    border-radius: 8px;
    background-color: var(--color-background);
    box-shadow: var(--shadow-elevation-2);
  }

  .form-container {
    display: flex;
    flex-direction: column;

    padding: 1rem;
    gap: 0.5rem;
  }

  .actions {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    border-top: 1px solid var(--color-border);
  }

  input {
    border: 0;
    border-radius: 0.25rem;
    padding: 0.5rem;
    font-size: 1.5rem;
  }

  .checkbox-group {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
</style>
