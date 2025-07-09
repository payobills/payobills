<script lang="ts">
  import Icon from "svelte-awesome";
  import type { IconDefinition } from "@fortawesome/free-solid-svg-icons";
  import { onMount } from "svelte";

  export let icon: IconDefinition;
  export let backgroundColor: string = "transparent";
  export let color: string = "white";
  export let scale = 1.5;
  export let style = "";

</script>

<!-- Ref: https://fontawesome.com/search?ip=classic&s=solid&o=r -->
<button on:click class="icon-button" >
  <Icon
    data={icon}
    {scale}
    style={`color: ${color};cursor: pointer; background-color: ${backgroundColor}; ${style}`}
  />
</button>

<style>
  button {
    background-color: transparent;
    margin: 0;
    padding: 0;
  }
</style>
