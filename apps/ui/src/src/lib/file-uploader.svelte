<script lang="ts">
  export let onFileAdded: (input: { files: any[] }) => void = (_) => {};

  let state:
    | "WAITING_FOR_INPUT"
    | "FILE_SELECTED"
    | "FILE_UPLOADING"
    | "FILE_UPLOAD_SUCCEEDED"
    | "FILE_UPLOAD_FAILED" = "WAITING_FOR_INPUT";
  let selectedFiles: any[];
</script>

<form
  on:submit|preventDefault={() => onFileAdded({ files: [selectedFiles[0]] })}
>
  <input type="file" bind:files={selectedFiles} />

  <div>
    {#if selectedFiles}
      Selected file: {selectedFiles[0].name}
    {/if}
  </div>

  <button type="submit">Upload</button>
</form>

<style>
  form {
    width: 100%;
  }

  input {
    margin-bottom: 1rem;
  }
</style>
