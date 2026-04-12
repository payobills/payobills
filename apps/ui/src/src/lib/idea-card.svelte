<script lang="ts">
import Icon from "svelte-awesome";
import { faLightbulb } from "@fortawesome/free-solid-svg-icons";
import IconButton from "$lib/icon-button.svelte";

export let idea: string;
</script>

<div class="idea-card">
  <IconButton
    icon={faLightbulb}
    scale={1}
    style={`border-radius: 4rem; color: #ffa029; padding: 0 1rem 0 .5rem; cursor: pointer; align-self: center`}
  />
  <p>{idea}</p>
</div>

<style>
  .idea-card {
    display: flex;
    padding: 0.625rem 0.75rem;
    background-color: rgba(251, 191, 36, 0.07);
    border: 1px solid rgba(251, 191, 36, 0.2);
    border-radius: 0.5rem;
    align-items: center;
    gap: 0.25rem;
    margin-bottom: 0.75rem;
  }

  p {
    margin: 0;
    color: rgba(251, 191, 36, 0.9);
    font-size: 0.75rem;
    line-height: 1.5;
  }
</style>
