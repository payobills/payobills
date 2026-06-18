<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import Button from "$lib/button.svelte";

  const { Story } = defineMeta({
    title: "Components/Button",
    component: Button,
    tags: ["autodocs"],
    argTypes: {
      state: {
        control: "select",
        options: ["DEFAULT", "LOADING", "SUCCESS", "ERROR"],
      },
    },
  });
</script>

<Story name="Default" args={{ state: "DEFAULT" }}>
  {#snippet children(args)}
    <div style="max-width: 300px;">
      <Button {...args}>
        {#snippet children()}
          Submit
        {/snippet}
      </Button>
    </div>
  {/snippet}
</Story>

<Story name="Loading" args={{ state: "LOADING" }}>
  {#snippet children(args)}
    <div style="max-width: 300px;">
      <Button {...args}>
        {#snippet children()}
          Submit
        {/snippet}
      </Button>
    </div>
  {/snippet}
</Story>

<Story name="Record Payment" args={{ state: "DEFAULT" }}>
  {#snippet children(args)}
    <div style="max-width: 300px;">
      <Button {...args}>
        {#snippet children()}
          Record Payment
        {/snippet}
      </Button>
    </div>
  {/snippet}
</Story>

<Story name="Add Transaction" args={{ state: "DEFAULT" }}>
  {#snippet children(args)}
    <div style="max-width: 300px;">
      <Button {...args}>
        {#snippet children()}
          Add Transaction
        {/snippet}
      </Button>
    </div>
  {/snippet}
</Story>
