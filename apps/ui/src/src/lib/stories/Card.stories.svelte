<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import Card from "$lib/card.svelte";

  const { Story } = defineMeta({
    title: "Components/Card",
    component: Card,
    tags: ["autodocs"],
    argTypes: {
      title: { control: "text" },
    },
  });
</script>

<Story name="With Title" args={{ title: "Recent Transactions" }}>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <Card {...args}>
        <p style="margin: 0; color: var(--color-neutral-content); font-size: 0.8125rem;">
          Card content goes here. This is a reusable container with an optional title header.
        </p>
      </Card>
    </div>
  {/snippet}
</Story>

<Story name="Without Title">
  {#snippet children()}
    <div style="max-width: 400px;">
      <Card>
        <p style="margin: 0; color: var(--color-neutral-content); font-size: 0.8125rem;">
          A card without a title — just a styled container.
        </p>
      </Card>
    </div>
  {/snippet}
</Story>

<Story name="With Rich Content" args={{ title: "Trip Details" }}>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <Card {...args}>
        <h2 style="margin: 0 0 0.25rem 0;">Weekend Getaway</h2>
        <p style="margin: 0; color: var(--color-neutral-content); font-size: 0.85rem; opacity: 0.65;">
          Jun 15, 2026 – Jun 18, 2026
        </p>
      </Card>
    </div>
  {/snippet}
</Story>
