<script module>
  import { defineMeta } from "@storybook/addon-svelte-csf";
  import Nav from "$lib/nav.svelte";
  import { nav } from "$lib/stores/nav";

  nav.set({ isOpen: true, title: "Payobills", link: "/" });

  const { Story } = defineMeta({
    title: "Navigation/Nav",
    component: Nav,
    tags: ["autodocs"],
  });
</script>

<Story name="Default">
  {#snippet children()}
    <div style="max-width: 420px; position: relative;">
      <Nav />
    </div>
  {/snippet}
</Story>
