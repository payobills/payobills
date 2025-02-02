<script lang="ts">
  export let bill: any;
  export let onBillStatementFormUpload: any;

  let selectedFiles: any;
</script>

<form on:submit|preventDefault={() => onBillStatementFormUpload({ bill, billStatementFile: selectedFiles[0]})}>
  <label
    >Choose file:
    <input type="file" bind:files={selectedFiles} /></label>
  {#if selectedFiles}
    Selected file: {selectedFiles.name}
  {:else}
    No file chosen
  {/if}

  <button type="submit">Submit</button>
</form>
