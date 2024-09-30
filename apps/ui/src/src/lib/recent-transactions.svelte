<script lang="ts">
  import { onMount } from "svelte";
  import { formatRelativeDate } from "../utils/format-relative-date";

  export let transactions: any[] = [];
  export let showViewAllCTA = true;
  export let showAllTransactions = false;

  let loaded = false;
  $: apexCharts = null;

  onMount(async () => {
    if ((window as any).ApexCharts) {
      loaded = true;
      return;
    }

    await load();
  });

  const load = async () => {
    const module = await import("apexcharts");
    (window as any).ApexCharts = module.default;
    apexCharts = module.default as any;
    loaded = true;
  };

  const chart = (node: any) => {
    if (!loaded) load();

    const orderedData = transactions.sort((a: any, b: any) => {
      return new Date(a.backDate).getTime() - new Date(b.backDate).getTime();
    });

    let allData = orderedData.map((p: any) => {
      return {
        x: Intl.DateTimeFormat(undefined, {
          day: '2-digit',
          month: "short",
          year: "2-digit",
        }).format(new Date(p.backDate).getTime()),
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

    let options: any = {
      colors: ["#7a98c5"],
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
        labels:{show:false}
      },
      yaxis: {
        labels: {
          show: false,
          formatter: (value: number) => `₹ ${value}`,
        },
      },
      dataLabels: {
        enabled: true,
        style: {
          colors: ["#96b7e8"],
        },
        formatter: (value: number) => `₹ ${value}`,
      },
    };

    let myChart = new (window as any).ApexCharts(node, options);
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
  <div class="title">
    <h1>Recent Transactions</h1>
    {#if showViewAllCTA}
      <a href="/transactions">view all</a>
    {/if}
  </div>

  {#if (showAllTransactions && ApexCharts)}
    <div use:chart></div>
  {/if}

  {#each showAllTransactions ? transactions : transactions.slice(0, 5) as transaction}
    <div class="recent-transaction">
      <div>
        <span>{transaction.merchant}</span>
        <span> • </span>
        <span class="paid-on"
          >{formatRelativeDate(new Date(transaction.backDate))}</span
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
