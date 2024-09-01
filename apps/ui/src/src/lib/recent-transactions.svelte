<script lang="ts">
  export let transactions: any[] = [];
</script>

<div class="container">



<h1>Recent Transactions</h1>

{#each transactions.slice(0, 5) as transaction}
  <div class="recent-transaction">
    <div>
      <span>{transaction.merchant}</span>
      <span> • </span>
      <span class="paid-on"
        >{Intl.DateTimeFormat(undefined, {
          day: "numeric",
          month: "short",
        }).format(new Date(transaction.backDateString).getTime())}</span
      >
    </div>
    <span
      >{new Intl.NumberFormat(undefined, {
        style: "currency",
        currency: "INR",
      }).format(transaction.amount)}</span
    >
  </div>
{/each}
</div>
<style>
.container {
    padding: 1rem;
    background-color: var(--primary-bg-color);
}

  .recent-transaction {
    display: flex;
    justify-content: space-between;
    margin: 1rem 0;
  }
</style>
