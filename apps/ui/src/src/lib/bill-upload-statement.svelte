<script lang="ts">
  export let bill: any;
  let selectedFiles: any;
  const onSubmit = () => {
    const formdata = new FormData();
    formdata.append(
      "tags",
      JSON.stringify({
        CorrelationID: "28",
        Type: "BILL_STATEMENT",
        Note: "AMEX November",
      })
    );

    formdata.append("file", selectedFiles[0], selectedFiles[0].name);

    fetch("/files", {
      method: "POST",
      body: formdata,
    })
      .then((response) => response.json())
      .catch((error) => console.error("Error:", error));
  };
</script>

<form on:submit|preventDefault={onSubmit}>
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
