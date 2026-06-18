<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import BottomNav from "$lib/bottom-nav.svelte";

  const { Story } = defineMeta({
    title: "Navigation/BottomNav",
    component: BottomNav,
    tags: ["autodocs"],
  });
</script>

<Story name="Default">
  {#snippet children()}
    <div style="position: relative; height: 120px; max-width: 420px; overflow: hidden;">
      <BottomNav />
    </div>
  {/snippet}
</Story>
