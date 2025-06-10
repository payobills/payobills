<script lang="ts">
  import { faClose, faFile } from "@fortawesome/free-solid-svg-icons";
  import IconButton from "./icon-button.svelte";

  export let onFilesChanged: (input: { files: FileList }) => void = (_) => {};
  export let editable = false;
  export let files: any[] = [];

  // let state:
  //   | "WAITING_FOR_INPUT"
  //   | "FILE_SELECTED"
  //   | "FILE_UPLOADING"
  //   | "FILE_UPLOAD_SUCCEEDED"
  //   | "FILE_UPLOAD_FAILED" = "WAITING_FOR_INPUT";
  let selectedFileList: FileList;

  // $: console.log("Selected files:", selectedFileList);
</script>

<form
  on:submit|preventDefault={() => onFilesChanged({ files: selectedFileList })}
>
  {#if editable}
    <div class="tile-item">
      <input
        id="file-input"
        type="file"
        bind:files={selectedFileList}
        hidden
        multiple
      />
      <label class="upload-label" for="file-input"><span>+</span></label>
    </div>
    {#each selectedFileList as selectedFile}
      <div class="tile-item tile-item--file">
        {#if selectedFile.type.startsWith("image/")}
          <img
            src={URL.createObjectURL(selectedFile)}
            alt={selectedFile.name}
            style="width: 100%; height: 100%; object-fit: cover;"
          />
        {:else}
          <IconButton
            icon={faFile}
            backgroundColor="var(--primary-bg-color)"
            color="black"
            scale={2}
          />
        {/if}
      </div>
    {/each}
  {/if}
  
  {#if files.length > 0}
    {#each files as selectedFile}
      <div class="tile-item tile-item--file">
        {#if selectedFile.mimeType.startsWith("image/")}
          <img
            src={selectedFile.downloadPath}
            alt={selectedFile.fileName}
            style="width: 100%; height: 100%; object-fit: cover;"
          />
        {:else}
          <IconButton
            icon={faFile}
            backgroundColor="var(--primary-bg-color)"
            color="black"
            scale={2}
          />
        {/if}
      </div>
    {/each}
  {/if}
</form>

<style>
  form {
    display: flex;
    flex-wrap: wrap;
    width: 100%;
  }

  input {
    margin-bottom: 1rem;
  }

  .tile-item {
    display: flex;
    width: calc((100% - 3 * 1rem - 4 * 2px) / 4); /* 4 tiles per row */
    margin-right: 1rem;
    margin-bottom: 1rem;
    justify-content: center;
    align-items: center;
    aspect-ratio: 1;
  }

  .tile-item--file {
    flex-direction: column;
    justify-content: center;
  }

  form > :nth-child(1) {
    font-size: 3.2rem;
    border-radius: 0.25rem;
    background: var(--primary-color);
    color: white;
    justify-content: center;
    align-items: center;
    border: 1px solid var(--color);
  }

  form > :nth-child(4n) {
    margin-right: 0;
  }

  .tile-item--file {
    border: 1px solid var(--color);
  }
</style>
