<script lang="ts">
  import Icon from "svelte-awesome";
  import type { IconDefinition } from "@fortawesome/free-solid-svg-icons";
  import { onMount } from "svelte";

  export let icon: IconDefinition;
  export const backgroundColor: string = "transparent";
  export const color: string = "white";
  export const scale = 1.5;
  export const style = "";
</script>

<!-- Ref: https://fontawesome.com/search?ip=classic&s=solid&o=r -->
<button on:click class="icon-button" style={`${style}; ${backgroundColor ? `background-color: ${backgroundColor}` :''}`}>
  <Icon
    data={icon}
    {scale}
    style={`fill: ${color}; cursor: pointer; ${backgroundColor ? `background-color: ${backgroundColor}` : ""}; `}
  />
</button>

<style>
  button {
    /* background-color: transparent; */
    margin: 0;
    padding: 1.6rem;
  }
</style>
