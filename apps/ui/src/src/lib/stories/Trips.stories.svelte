<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import Trips from "$lib/trips.svelte";

  const { Story } = defineMeta({
    title: "Components/Trips",
    component: Trips,
    tags: ["autodocs"],
  });
</script>

<Story
  name="With Trips"
  args={{
    trips: [
      { id: "1", title: "Goa Beach Trip", startDate: "2026-06-15", endDate: "2026-06-20" },
      { id: "2", title: "Office Retreat", startDate: "2026-07-01", endDate: "2026-07-03" },
      { id: "3", title: "Family Vacation", startDate: "2026-08-10", endDate: "2026-08-18" },
    ],
    fetching: false,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <Trips {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="With New Trip Button"
  args={{
    trips: [
      { id: "1", title: "Weekend Getaway", startDate: "2026-06-20", endDate: "2026-06-22" },
    ],
    fetching: false,
    onNewTrip: () => alert("New trip clicked"),
  }}
>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <Trips {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="Empty State"
  args={{
    trips: [],
    fetching: false,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <Trips {...args} />
    </div>
  {/snippet}
</Story>

<Story
  name="Loading"
  args={{
    trips: [],
    fetching: true,
  }}
>
  {#snippet children(args)}
    <div style="max-width: 400px;">
      <Trips {...args} />
    </div>
  {/snippet}
</Story>
