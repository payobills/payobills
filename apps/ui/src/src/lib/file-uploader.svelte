<script lang="ts">
  import { faClose, faFile } from "@fortawesome/free-solid-svg-icons";
  import IconButton from "./icon-button.svelte";

  export let onFileAdded: (input: { file: File }) => void = (_) => {};
  export let onFileRemoved: (input: { file: File }) => void = (_) => {};
  export let editable = false;
  export let files: any[] = [];
</script>

<form on:submit|preventDefault={() => {}}>
  {#if editable}
    <div class="tile-item tile-item--upload-button">
      <input
        id="file-input"
        type="file"
        hidden
        on:change={(e: any) => {
          // console.log("change events", e);
          // console.log('adding', e?.target?.files[0])

          if (e?.target?.files == null) return;
          onFileAdded({ file: e?.target?.files[0] });
        }}
      />
      <label class="upload-label" for="file-input"><span>+</span></label>
    </div>
    <div class="tile-item--filler tile-item--receipt-remove-icon"></div>
    <!-- {#each selectedFileList as selectedFile}
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
        on:click={() => onFileRemoved({file: selectedFile})}
        />
        {/if}
      </div>
      <div class='tile-item--receipt-remove-icon'>
        <IconButton
        icon={faClose}
        color="white"
        scale={0.75}
        style={"padding: .125rem"}
        />
      </div>
    {/each} -->
  {/if}

  <!-- {#if files.length > 0} -->
  {#each files as selectedFile}
    <div class="tile-item tile-item--file">
      {#if selectedFile?.mimeType?.startsWith("image/")}
        <img
          src={selectedFile.downloadPath}
          alt={selectedFile.fileName}
          style="width: 100%; height: 100%; object-fit: cover;"
        />
      {:else if selectedFile?.type?.startsWith("image/")}
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
    <div
      class={`tile-item--receipt-remove-icon ${editable ? "" : "tile-item--filler"}`}
    >
      {#if editable}
        <IconButton
          icon={faClose}
          color="white"
          scale={0.75}
          style={"padding: .125rem"}
           on:click={() => {
            console.log('trying to remove file', selectedFile)
            onFileRemoved({ file: selectedFile })
          }}
        />
      {/if}
    </div>
  {/each}
  <!-- {/if} -->
</form>

<style>
  form {
    display: flex;
    flex-wrap: wrap;
    width: 100%;
  }

  input {
    margin-bottom: 2rem;
  }

  .tile-item--filler {
    width: 1rem;
  }
  .tile-item {
    display: flex;
    width: calc((100% - 4 * 2rem - 4 * 2px) / 4);
    margin-bottom: 2rem;
    justify-content: center;
    align-items: center;
    aspect-ratio: 1;
  }
  .tile-item--receipt-remove-icon {
    position: relative;
    right: 1rem;
    top: -0.5rem;
  }

  :global(.tile-item--receipt-remove-icon > .icon-button) {
    background-color: var(--primary-color);
    padding: 0.25rem;
    height: 2rem;
    width: 2rem;
    border-radius: 2rem;
  }

  .tile-item--upload-button {
    font-size: 3.2rem;
    margin-right: 1rem;
    border-radius: 0.25rem;
    background: var(--primary-color);
    color: white;
    justify-content: center;
    align-items: center;
    border: 1px solid var(--color);
  }

  .tile-item--file {
    flex-direction: column;
    justify-content: center;
  }

  form > :nth-child(4n) {
    margin-right: 0;
  }

  .tile-item--file {
    border: 1px solid var(--color);
  }
</style>
