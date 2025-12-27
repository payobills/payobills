<script lang="ts">
  import { queryStore, gql } from "@urql/svelte";
  import { billsUrql } from "$lib/stores/urql";
  import IconButton from "$lib/icon-button.svelte";
  import { page } from "$app/stores";
  import PaymentTimelinePill from "$lib/payment-timeline-pill.svelte";
  import Nav from "$lib/nav.svelte";
  import Card from "$lib/card.svelte";
  import { onMount } from "svelte";
  import BillUploadStatement from "$lib/bill-upload-statement.svelte";
  import { nav } from "$lib/stores/nav"
  import BillStatements from "$lib/bills/bill-statements.svelte";
  import { browser } from "$app/environment";
  // import type { BillDTO } from "$lib/types";
  import RecordPaymentForm from "$lib/record-payment-form.svelte";
  import UiDrawer from "$lib/ui-drawer.svelte";
  import { LiteBillService } from "$utils/lite/lite-bills.service";
    import type { IBillsService } from "../../../utils/interfaces/bills-service.interface";
    import { liteServices, setupLiteServices } from "../../../lib/stores/lite-services";
    import { goto } from "$app/navigation";
  import { faEdit } from "@fortawesome/free-solid-svg-icons";
    import { withOrdinalSuffix } from "../../../utils/ordinal-suffix";

  let billId: any;
  let billByIdQuery: any;
  let refreshKey: number = Date.now();
  let billsService: IBillsService 

  let showUploadStatementSection = false;
  let showRecordPayment = false;
  let uploadStatementResult = undefined;

  onMount(async () => {
    nav.update(prev => ({...prev, isOpen: true }))

    billId = new URLSearchParams(window.location.search)?.get('id')
    if ((window as any).ApexCharts) {
      loaded = true;
      return;
    }
    await load();
  });

  interface FileUploadResult {
    data: {
      id: string;
    };
  }
  $: billByIdQuery = (billId && $liteServices?.billsService) ? $liteServices.billsService.queryBillById(billId): null
  $: billStatementsQuery = (billId && $liteServices?.billStatementsService) ? $liteServices.billStatementsService.queryBillStatementsByBillIds([billId]): null

  async function markPaid() {
    const markPaidQuery = $billsUrql
      .mutation(
        gql`
          mutation markPayment($billId: UUID!) {
            markPayment(dto: { id: $billId }) {
              id
              createdAt
              updatedAt
              billingPeriod {
                start
                end
              }
            }
          }
        `,
        { billId }
      )
      .toPromise();

    let { error } = await markPaidQuery;

    if (error) {
      console.error(error);
      return;
    }

    refreshKey = Date.now();
  }

  let loaded = false;
  let ApexCharts: any;

  $: if (browser) {
    billId = $page.url.searchParams.get("id");
  }


  const onBillStatementFormUpload = async (inputs: {
    bill: BillDTO;
    billStatementFile: File;
    billPeriodDetails: any;
  }) => {
    try {
      const formdata = new FormData();
      formdata.append(
        "tags",
        JSON.stringify({
          CorrelationID: inputs.bill.id,
          Type: "BILL_STATEMENT",
          Note: inputs.billStatementFile.name,
        })
      );

      formdata.append(
        "file",
        inputs.billStatementFile,
        inputs.billStatementFile.name
      );

      const response = await fetch("/files/files", {
        method: "POST",
        body: formdata,
      });

      if (!response.ok) {
        throw new Error(`File upload failed: ${response.statusText}`);
      }

      const fileUploadResult = (await response.json()) as FileUploadResult;

      const { error } = await $billsUrql
        .mutation(
          gql`
            mutation (
              $fileId: Int!
              $billId: Int!
              $billPeriodStartDate: String!
              $billPeriodEndDate: String!
            ) {
              addBillStatement(
                dto: {
                  notes: ""
                  startDate: $billPeriodStartDate
                  endDate: $billPeriodEndDate
                  file: { id: $fileId }
                  bill: { id: $billId }
                }
              ) {
                id
                startDate
                endDate
                notes
                bill {
                  id
                  name
                }
              }
            }
          `,
          {
            billId: +inputs.bill.id,
            fileId: +fileUploadResult.data.id,
            billPeriodStartDate: Intl.DateTimeFormat("en-CA", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
            }).format(inputs.billPeriodDetails.billStartDate.getTime()),
            billPeriodEndDate: Intl.DateTimeFormat("en-CA", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
            }).format(inputs.billPeriodDetails.billEndDate.getTime()),
          }
        )
        .toPromise();

      if (error) {
        throw error;
      }

      uploadStatementResult = true;
      refreshKey = Date.now(); // Refresh the query to show the new statement
    } catch (error) {
      console.error("Error:", error);
      uploadStatementResult = false;
      throw error;
    }
  };

  const load = async () => {
    const module = await import("apexcharts");
    ApexCharts = module.default;
    (window as any).ApexCharts = ApexCharts;
  };

  const chart = (node: any) => {
    if (!loaded) load();

    const orderedData = [
      $billByIdQuery.data.payments,
      $billByIdQuery.data.billStatements,
    ]
      .flat()
      .sort((a: any, b: any) => {
        return new Date(a.paidAt).getTime() - new Date(b.paidAt).getTime();
      });

    let allData = orderedData.map((p: any) => {
      return {
        x: Intl.DateTimeFormat(undefined, {
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
      },
      yaxis: {
        labels: {
          show: false,
          formatter: (value: number) => `₹ ${value}`,
        },
      },
      dataLabels: {
        enabled: true,
        // textAnchor: 'start',
        style: {
          colors: ["#000"],
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

  const onRecordPayment = () => {

  }
</script>

<Card>
  <div class="content">
    {#if $billByIdQuery?.fetching}
      <p>Loading...</p>
    {:else if $billByIdQuery?.error}
      <p>🙆‍♂️ Uh oh! Unable to fetch your bill!</p>
    {:else if $billByIdQuery?.data}
      <section class='heading-section'>
      <h1>{$billByIdQuery?.data.name}</h1>
            <IconButton
              icon={faEdit}
              color="white"
              scale={1.5}
              style="padding: 0.25rem"
              on:click={() => {
                goto(`bills/add?existing-bill-id=${$billByIdQuery?.data.id}`)
              }}
            />
      </section>
      {#if $billByIdQuery?.data.billingDate || $billByIdQuery?.data.payByDate}
      <section class="bill-details">
       {#if $billByIdQuery?.data.billingDate}
      
        <div>Billing Date</div>
  <div>the <span class="bill-detail">{`${withOrdinalSuffix($billByIdQuery?.data?.billingDate)}`}</span> of every month 
    </div>
      {/if}

       {#if $billByIdQuery?.data.payByDate}
      
        <div>Pay by Date</div>
        <div>the <span class="bill-detail">{`${withOrdinalSuffix($billByIdQuery?.data?.payByDate)}`}</span> of every month 
    </div>
      {/if}

      </section>
      {/if}
      <h2>past payments</h2>
       {#if ($billByIdQuery.data.payments || []).length == 0} 
        <p>we don't see any payments made for this bill. 😞</p>
  {:else}
      <div use:chart></div>
      {#each ($billByIdQuery.data.payments || []) as payment}
        <p class="payment">
    flex-direction: ;
          {#if payment.amount}
            <!-- TODO: Get currency from user, suggest based on Intl -->
            <span class="amount"
              >{new Intl.NumberFormat(undefined, {
                style: "currency",
                currency: "INR",
              }).format(payment.amount)} (manually entered)</span
            >
          {:else}
            <span class="amount--unknown">Unknown Amount</span>
          {/if}
          <span class="paid-on"
            >{Intl.DateTimeFormat(undefined, {
              month: "long",
              year: "2-digit",
            }).format(new Date(payment.paidAt).getTime())}</span
          >
        </p>
      {/each}

      <BillStatements
        bill={{ id: billId }}
        statements={$billByIdQuery.data.billStatements}
      />
{/if}

      {#if showUploadStatementSection}
        <BillUploadStatement
          bill={$billByIdQuery.data}
          {onBillStatementFormUpload}
        />
      {/if}


      <div class="actions">
        {#if !showUploadStatementSection}
          <button on:click={() => (showUploadStatementSection = true)}
            >upload statement</button
          >
        {/if}
      </div>
    {/if}
  </div>
</Card>

<style>
  .heading-section {
    display: flex;
    align-items: center;
  }

  h1 {
    color: var(--primary-color);
    font-size: 1.2rem;
    font-weight: 600;
    flex-grow: 1;
  }

  h2 {
    margin-top: 2rem;
  }

  p {
    font-size: 0.8rem;
  }
  .content {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }
  .actions {
    display: flex;
    flex-grow: 1;
    justify-content: flex-end;
    flex-direction: column;
  }

  .actions > button {
    margin: 0.25rem 0;
    width: 100%;
  }

  .payment {
    display: flex;
  }

  .amount,
  .amount--unknown {
    flex-grow: 1;
  }

  span.bill-detail {
    font-weight: 800;
  }
</style>
