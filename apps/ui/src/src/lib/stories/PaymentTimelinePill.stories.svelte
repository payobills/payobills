<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import PaymentTimelinePill from "$lib/payment-timeline-pill.svelte";

  const { Story } = defineMeta({
    title: "Components/PaymentTimelinePill",
    component: PaymentTimelinePill,
    tags: ["autodocs"],
    argTypes: {
      showLabel: { control: "boolean" },
    },
  });
</script>

<Story
  name="Normal Cycle (billing before pay-by)"
  args={{
    item: { name: "HDFC Credit Card", billingDate: 5, payByDate: 25 },
    showLabel: true,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <PaymentTimelinePill {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="Wrapped Cycle (billing after pay-by)"
  args={{
    item: { name: "SBI Credit Card", billingDate: 28, payByDate: 15 },
    showLabel: true,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <PaymentTimelinePill {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="Early Month Due"
  args={{
    item: { name: "Internet Bill", billingDate: 1, payByDate: 10 },
    showLabel: true,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <PaymentTimelinePill {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="Without Label"
  args={{
    item: { name: "Electric Bill", billingDate: 10, payByDate: 20 },
    showLabel: false,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <PaymentTimelinePill {...args} />
    </div>
  {/snippet}
</Story>

<Story name="Multiple Bills">
  {#snippet children()}
    <div style="max-width: 400px;">
      <PaymentTimelinePill item={{ name: "HDFC Credit Card", billingDate: 5, payByDate: 25 }} showLabel={true} />
      <PaymentTimelinePill item={{ name: "SBI Credit Card", billingDate: 28, payByDate: 15 }} showLabel={true} />
      <PaymentTimelinePill item={{ name: "Internet", billingDate: 1, payByDate: 10 }} showLabel={true} />
      <PaymentTimelinePill item={{ name: "Electricity", billingDate: 15, payByDate: 30 }} showLabel={true} />
    </div>
  {/snippet}
</Story>
