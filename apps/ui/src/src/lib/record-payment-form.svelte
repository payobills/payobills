<script lang="ts">
  import BillPayment from "./bills/bill-payment.svelte";

  export let bill: any;
</script>

<BillPayment {bill} billingStatements={[]} />
