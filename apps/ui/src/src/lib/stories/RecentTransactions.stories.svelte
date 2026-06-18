<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import RecentTransactions from "$lib/recent-transactions.svelte";

  const sampleTransactions = [
    { id: "1", merchant: "Swiggy", amount: 450, paidAt: new Date(2026, 5, 18, 12, 30).toISOString(), currency: "INR" },
    { id: "2", merchant: "Amazon", amount: 2999, paidAt: new Date(2026, 5, 17, 15, 0).toISOString(), currency: "INR" },
    { id: "3", merchant: "Uber", amount: 180, paidAt: new Date(2026, 5, 17, 9, 45).toISOString(), currency: "INR" },
    { id: "4", merchant: "Netflix", amount: 649, paidAt: new Date(2026, 5, 16, 0, 0).toISOString(), currency: "INR" },
    { id: "5", merchant: "Zepto", amount: 320, paidAt: new Date(2026, 5, 16, 18, 20).toISOString(), currency: "INR" },
    { id: "6", merchant: "Starbucks", amount: 550, paidAt: new Date(2026, 5, 15, 10, 0).toISOString(), currency: "INR" },
    { id: "7", merchant: "BigBasket", amount: 1200, paidAt: new Date(2026, 5, 15, 14, 30).toISOString(), currency: "INR" },
    { id: "8", merchant: "Zomato", amount: 380, paidAt: new Date(2026, 5, 14, 20, 0).toISOString(), currency: "INR" },
  ];

  const { Story } = defineMeta({
    title: "Components/RecentTransactions",
    component: RecentTransactions,
    tags: ["autodocs"],
    argTypes: {
      showViewAllCTA: { control: "boolean" },
      showAllTransactions: { control: "boolean" },
      showRecentSpends: { control: "boolean" },
      showGraph: { control: "boolean" },
      showTotalSpend: { control: "boolean" },
      showDisclaimer: { control: "boolean" },
      groupTransactionByDate: { control: "boolean" },
      initialShowCount: { control: "number" },
    },
  });
</script>

<Story
  name="Default (Grouped by Date)"
  args={{
    transactions: sampleTransactions,
    showViewAllCTA: true,
    showTotalSpend: true,
    groupTransactionByDate: true,
    showDisclaimer: true,
    initialShowCount: 5,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 420px;">
      <RecentTransactions {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="Flat List"
  args={{
    transactions: sampleTransactions,
    showViewAllCTA: false,
    showTotalSpend: true,
    groupTransactionByDate: false,
    showDisclaimer: false,
    initialShowCount: 5,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 420px;">
      <RecentTransactions {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="With Recent Spends"
  args={{
    transactions: sampleTransactions,
    title: "Your Recent Spends",
    showRecentSpends: true,
    showViewAllCTA: true,
    showTotalSpend: false,
    groupTransactionByDate: false,
    showDisclaimer: false,
    initialShowCount: 5,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 420px;">
      <RecentTransactions {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="Empty State"
  args={{
    transactions: [],
    showViewAllCTA: true,
    showTotalSpend: false,
    groupTransactionByDate: true,
    showDisclaimer: true,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 420px;">
      <RecentTransactions {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="Custom Title"
  args={{
    transactions: sampleTransactions.slice(0, 3),
    title: "June 2026",
    showViewAllCTA: false,
    showTotalSpend: true,
    groupTransactionByDate: false,
    showDisclaimer: false,
    initialShowCount: 10,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 420px;">
      <RecentTransactions {...args} />
    </div>
  {/snippet}
</Story>
