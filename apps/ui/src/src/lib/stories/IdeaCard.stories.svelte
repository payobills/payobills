<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import IdeaCard from "$lib/idea-card.svelte";

  const { Story } = defineMeta({
    title: "Components/IdeaCard",
    component: IdeaCard,
    tags: ["autodocs"],
    argTypes: {
      idea: { control: "text" },
    },
  });
</script>

<Story name="Payment Suggestion" args={{ idea: "Pay all bills together by paying between June 5 and June 12!" }}>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <IdeaCard {...args} />
    </div>
  {/snippet}
</Story>

<Story name="Short Tip" args={{ idea: "Set up autopay to never miss a due date." }}>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <IdeaCard {...args} />
    </div>
  {/snippet}
</Story>

<Story name="Long Text" args={{ idea: "You could save up to ₹500 this month by consolidating your electricity and internet payments into a single billing cycle with your credit card." }}>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <IdeaCard {...args} />
    </div>
  {/snippet}
</Story>
