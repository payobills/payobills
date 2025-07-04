<script lang="ts">
  import { onMount } from "svelte";
  import { formatRelativeDate } from "../utils/format-relative-date";

  export let transactions: any[] = [];
  $: orderedTransactions = transactions.reduce(
    (agg: any[], currTransaction) => {
      let duplicateTransaction = agg.findIndex(
        (p) => p.id === currTransaction.id
      );
      if (duplicateTransaction === -1) {
        return [...agg, currTransaction];
      }

      return agg;
    },
    []
  );
  export let showViewAllCTA = true;
  export let showAllTransactions = false;
  export let showGraph = false;
  export let title: string | undefined = undefined;
  export let totalSpend = 0;
  export let initialShowCount = 5;

  $: ApexCharts = undefined;

  onMount(async () => {
    if ((window as any).ApexCharts) {
      ApexCharts = (window as any).ApexCharts;
      return;
    }

    await load();
  });

  const load = async () => {
    const module = await import("apexcharts");
    (window as any).ApexCharts = module.default;
    ApexCharts = module.default as any;
  };

  const chart = (node: any, transactions: any[]) => {
    if (!ApexCharts) return;

    const orderedData = transactions.sort((a: any, b: any) => {
      return new Date(a.paidAt).getTime() - new Date(b.paidAt).getTime();
    });

    let allData = orderedData.map((p: any) => {
      return {
        x: Intl.DateTimeFormat(undefined, {
          day: "2-digit",
          month: "short",
          year: "2-digit",
        }).format(new Date(p.paidAt).getTime()),
        y: p.amount,
        note: `${p.amount}`,
      };
    });

    let data = allData.reduce(
      (accumulator: any[], current: any, index: number) => {
        if (index == 0) return [current];

        if (current.x == accumulator[accumulator.length - 1].x) {
          let last = accumulator[accumulator.length - 1];
          return [
            ...accumulator.slice(0, -1),
            {
              y: last.y + current.y,
              x: last.x,
              note: `${last.note} - + ${current.y}`,
            },
          ];
        }

        return [
          ...accumulator,
          {
            x: current.x,
            y: current.y,
            note: current.note,
          },
        ];
      },
      []
    );

    totalSpend = data.reduce((acc: number, p: any) => acc + p.y, 0);

    let options: any = {
      colors: ["var(--primary-color)"],
      legend: {
        show: false,
      },
      stroke: {
        curve: "smooth",
        width: 2,
      },
      chart: {
        type: "area",
      },
      series: [
        {
          name: "payments",
          data: data.map((p: any) => p.y),
        },
      ],
      xaxis: {
        categories: data.map((p: any) => p.x),
        labels: { show: false },
      },
      yaxis: {
        labels: {
          show: false,
          formatter: (value: number) =>
            `₹ ${Intl.NumberFormat(undefined, {
              style: "decimal",
            }).format(value)}`,
        },
      },
      dataLabels: {
        enabled: true,
        style: {
          colors: ["#5e5e5e"],
        },
        formatter: (value: number) =>
          `₹ ${Intl.NumberFormat(undefined, {
            style: "decimal",
          }).format(value)}`,
      },
    };

    let myChart = new (ApexCharts as any)(node, options);
    myChart.render();

    return {
      update(options: any) {
        myChart.updateOptions(options);
      },
      destroy() {
        myChart.destroy();
      },
    };
  };
</script>

<div class="container">
  <div class={`title ${title === "" ? "title--hidden" : ""}`}>
    {#if title !== undefined}
      <h1>{title}</h1>
    {:else}
      <h1>Recent Transactions</h1>
    {/if}
    {#if showViewAllCTA}
      <a href={`/transactions`}>view all</a>
    {/if}
  </div>

  {#if transactions.length === 0}
    <p>We don't see any transactions this month so far...</p>
  {/if}

  {#if showGraph && transactions.length > 0 && ApexCharts}
    <div use:chart={transactions}></div>

    <p class="disclaimer">
      It might take upto an hour for latest transactions to show up here...
    </p>
  {/if}

  <div class="transaction-card">
    <div class="recent-transaction">
      <div class="non-amount-details">
        <span>Total spend</span>
      </div>
      <span
        >{new Intl.NumberFormat(undefined, {
          style: "currency",
          currency: "INR",
        }).format(totalSpend)}</span
      >
    </div>
  </div>
  <hr />

  {#each showAllTransactions ? orderedTransactions : orderedTransactions.slice(0, initialShowCount) as transaction (transaction.id)}
    <a class="transaction-card" href={`/transaction/${transaction.id}`}>
      <div class="recent-transaction">
        <div class="non-amount-details">
          {#if transaction.merchant !== null}
            <span>{transaction.merchant}</span>
          {:else}
            <span>Unknown</span>
          {/if}
          <span class="paid-on"
            >{formatRelativeDate(new Date(transaction.paidAt))}</span
          >
        </div>
        {#if transaction.amount !== null}
          <span
            >{new Intl.NumberFormat(undefined, {
              style: "currency",
              currency: "INR",
            }).format(transaction.amount)}</span
          >
        {:else}
          <span>-</span>
        {/if}
      </div>
    </a>
  {/each}

  {#if showViewAllCTA}
    <p>
      Not seeing a transaction here? You can
      <a href={`/transaction/add`} class='transaction-add-cta'>add one</a> manually too.
    </p>
  {/if}
</div>

<style>
  .transaction-add-cta {
    font-weight: 800;
  }

  .transaction-card {
    all: unset;
  }

  .container {
    background-color: var(--primary-bg-color);
    padding-bottom: 0;
    flex-grow: 1;
  }

  .title--hidden {
    height: 0;
  }

  .recent-transaction {
    display: flex;
    justify-content: space-between;
    margin: 1rem 0;
  }

  .recent-transaction span {
    font-size: 0.75rem;
  }
  .non-amount-details {
    display: flex;
    flex-direction: column;
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

  .disclaimer {
    font-size: 0.75rem;
  }
</style>
