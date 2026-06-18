<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import IconButton from "$lib/icon-button.svelte";
  import {
    faPlus,
    faTrash,
    faEdit,
    faClose,
    faCheck,
    faHouse,
    faFileInvoiceDollar,
    faRoute,
    faListUl,
  } from "@fortawesome/free-solid-svg-icons";

  const { Story } = defineMeta({
    title: "Components/IconButton",
    component: IconButton,
    tags: ["autodocs"],
    argTypes: {
      scale: { control: { type: "range", min: 0.5, max: 3, step: 0.25 } },
      color: { control: "color" },
      backgroundColor: { control: "color" },
    },
  });
</script>

<Story name="Default" args={{ icon: faPlus, scale: 1.5, color: "white", backgroundColor: "transparent" }}>
  {#snippet children(args)}
    <IconButton {...args} />
  {/snippet}
</Story>

<Story name="With Background" args={{ icon: faPlus, scale: 1.5, color: "white", backgroundColor: "var(--color-primary)" }}>
  {#snippet children(args)}
    <IconButton {...args} />
  {/snippet}
</Story>

<Story name="Small" args={{ icon: faClose, scale: 0.75, color: "grey" }}>
  {#snippet children(args)}
    <IconButton {...args} />
  {/snippet}
</Story>

<Story name="Icon Gallery">
  {#snippet children()}
    <div style="display: flex; gap: 1rem; flex-wrap: wrap; align-items: center;">
      <div style="display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
        <IconButton icon={faHouse} scale={1.2} color="var(--color-neutral-content)" />
        <span style="font-size: 0.65rem; color: var(--color-neutral-content);">Home</span>
      </div>
      <div style="display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
        <IconButton icon={faListUl} scale={1.2} color="var(--color-neutral-content)" />
        <span style="font-size: 0.65rem; color: var(--color-neutral-content);">Txns</span>
      </div>
      <div style="display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
        <IconButton icon={faRoute} scale={1.2} color="var(--color-neutral-content)" />
        <span style="font-size: 0.65rem; color: var(--color-neutral-content);">Trips</span>
      </div>
      <div style="display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
        <IconButton icon={faFileInvoiceDollar} scale={1.2} color="var(--color-neutral-content)" />
        <span style="font-size: 0.65rem; color: var(--color-neutral-content);">Bills</span>
      </div>
      <div style="display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
        <IconButton icon={faPlus} scale={1.2} color="var(--color-neutral-content)" />
        <span style="font-size: 0.65rem; color: var(--color-neutral-content);">Add</span>
      </div>
      <div style="display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
        <IconButton icon={faEdit} scale={1.2} color="var(--color-neutral-content)" />
        <span style="font-size: 0.65rem; color: var(--color-neutral-content);">Edit</span>
      </div>
      <div style="display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
        <IconButton icon={faTrash} scale={1.2} color="var(--color-error)" />
        <span style="font-size: 0.65rem; color: var(--color-neutral-content);">Delete</span>
      </div>
      <div style="display: flex; flex-direction: column; align-items: center; gap: 0.5rem;">
        <IconButton icon={faCheck} scale={1.2} color="var(--color-success)" />
        <span style="font-size: 0.65rem; color: var(--color-neutral-content);">Check</span>
      </div>
    </div>
  {/snippet}
</Story>
