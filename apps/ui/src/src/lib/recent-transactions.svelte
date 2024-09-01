<script lang="ts">
  export let transactions: any[] = [];
  export let showViewAllCTA = true;
  export let showAllTransactions = false;
</script>

<div class="container">
  <div class="title">
    <h1>Recent Transactions</h1>
    {#if showViewAllCTA}
    <a href="/transactions">view all</a>
    {/if}
  </div>

  {#each (showAllTransactions ? transactions : transactions.slice(0, 5)) as transaction}
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
    padding-bottom: 0;
  }

  .recent-transaction {
    display: flex;
    justify-content: space-between;
    margin: 1rem 0;
  }

  .title {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .title a {
    font-size: 0.8rem;
    color: var(--primary-color);
    font-weight: 400;
  }
</style>
