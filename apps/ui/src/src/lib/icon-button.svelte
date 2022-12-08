<script lang="ts">
      import Icon from "svelte-awesome/components/Icon.svelte";
      import type { IconDefinition } from "@fortawesome/free-solid-svg-icons";

      export let icon: IconDefinition;
      export let backgroundColor: string;
      export let rounded: boolean;

</script>

<button on:click>
      <Icon
            data={icon}
            scale={1.5}
            style={`border-radius: 4rem; color: white; padding: 1rem; cursor: pointer; background-color: ${backgroundColor}`}
      />
</button>

<style>
      button {
            background-color: transparent;
            margin: 0;
            padding: 0;
      }
</style>
