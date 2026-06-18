<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import FileUploader from "$lib/file-uploader.svelte";

  const { Story } = defineMeta({
    title: "Components/FileUploader",
    component: FileUploader,
    tags: ["autodocs"],
    argTypes: {
      editable: { control: "boolean" },
    },
  });
</script>

<Story name="Editable (Empty)" args={{ editable: true, files: [] }}>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <FileUploader {...args} />
    </div>
  {/snippet}
</Story>

<Story name="Read Only with Files" args={{ editable: false, files: [
  { fileName: "receipt.pdf", mimeType: "application/pdf", downloadPath: "#" },
] }}>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <FileUploader {...args} />
    </div>
  {/snippet}
</Story>

<Story name="Editable with Files" args={{ editable: true, files: [
  { fileName: "scan.pdf", mimeType: "application/pdf", downloadPath: "#" },
] }}>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <FileUploader {...args} />
    </div>
  {/snippet}
</Story>
