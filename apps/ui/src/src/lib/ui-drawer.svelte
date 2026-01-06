<script lang="ts">
  import { uiDrawer } from "./stores/ui-drawer";

  const { children, onClose } = $props();

  $effect(() => {
    uiDrawer.update((curr) => ({
      ...curr,
      content: children,
      onClose
    }));
  });

</script>
