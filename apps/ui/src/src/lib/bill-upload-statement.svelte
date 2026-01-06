<script lang="ts">
  import type { BillDTO } from '$lib/types';

  export let bill: BillDTO;
  export let onBillStatementFormUpload: any;

  let rightNow = new Date();
  let billPeriod: string = `${rightNow.getFullYear()}-${rightNow.getMonth() + 1}`;
  $:  billPeriodDetails ={
      billStartDate: bill.billingDate ? new Date(+billPeriod.split("-")[0], +billPeriod.split("-")[1] - 2 , bill.billingDate + 1) : new Date('invalid'),
      billEndDate: bill.billingDate ? new Date(+billPeriod.split("-")[0], +billPeriod.split("-")[1] - 1, bill.billingDate) : new Date('invalid'),
  }

  let selectedFiles: any;
</script>

<form
  on:submit|preventDefault={() =>
    onBillStatementFormUpload({ bill, billStatementFile: selectedFiles[0], billPeriodDetails })}
>
  <label for="billPeriod"
    >Bill Period:
    <input id="billPeriod" type="month" bind:value={billPeriod} />
  </label>

  {JSON.stringify(billPeriod)}
  
  <p>startdate: {Number.isNaN(billPeriodDetails.billStartDate.getTime())?'we cooked':billPeriodDetails.billStartDate}</p>
  <p>enddate: {Number.isNaN(billPeriodDetails.billEndDate.getTime())?'we cooked':billPeriodDetails.billEndDate}</p>

  <br>

  <label
    >Choose file:
    <input type="file" bind:files={selectedFiles} /></label
  >
  {#if selectedFiles}
    Selected file: {selectedFiles.name}
  {:else}
    No file chosen
  {/if}

  <button type="submit">Submit</button>
</form>
