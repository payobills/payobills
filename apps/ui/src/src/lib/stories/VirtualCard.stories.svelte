<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import VirtualCard from "$lib/virtual-card.svelte";

  const { Story } = defineMeta({
    title: "Components/VirtualCard",
    component: VirtualCard,
    tags: ["autodocs"],
    argTypes: {
      title: { control: "text" },
    },
  });
</script>

<Story name="Default" args={{ title: "HDFC Credit Card" }}>
  {#snippet children(args)}
    <div style="max-width: 360px;">
      <VirtualCard {...args} />
    </div>
  {/snippet}
</Story>

<Story name="SBI Card" args={{ title: "SBI Debit Card" }}>
  {#snippet children(args)}
    <div style="max-width: 360px;">
      <VirtualCard {...args} />
    </div>
  {/snippet}
</Story>

<Story name="Narrow Container" args={{ title: "Amex Platinum" }}>
  {#snippet children(args)}
    <div style="max-width: 220px;">
      <VirtualCard {...args} />
    </div>
  {/snippet}
</Story>
