<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import BillStatements from "$lib/bills/bill-statements.svelte";

  const bill = {
    id: "bill-1",
    name: "HDFC Credit Card",
    billingDate: 5,
    payByDate: 25,
    isEnabled: true,
    createdAt: new Date(),
    updatedAt: new Date(),
    payments: [],
  };

  const { Story } = defineMeta({
    title: "Bills/BillStatements",
    component: BillStatements,
    tags: ["autodocs"],
  });
</script>

<Story name="With Statements">
  {#snippet children()}
    <div style="max-width: 400px;">
      <BillStatements
        {bill}
        statements={[
          { id: "s1", startDate: "2026-06-05", endDate: "2026-07-05" },
          { id: "s2", startDate: "2026-05-05", endDate: "2026-06-05" },
          { id: "s3", startDate: "2026-04-05", endDate: "2026-05-05" },
        ]}
      />
    </div>
  {/snippet}
</Story>

<Story name="Empty">
  {#snippet children()}
    <div style="max-width: 400px;">
      <BillStatements {bill} statements={[]} />
    </div>
  {/snippet}
</Story>

<Story name="Unknown Billing Period">
  {#snippet children()}
    <div style="max-width: 400px;">
      <BillStatements
        {bill}
        statements={[
          { id: "s1", startDate: null, endDate: null },
          { id: "s2", startDate: "2026-05-05", endDate: "2026-06-05" },
        ]}
      />
    </div>
  {/snippet}
</Story>
