<script lang="ts">
import { faLightbulb } from "@fortawesome/free-solid-svg-icons";
import Icon from "svelte-awesome";

export let idea: string;
</script>

<div class="idea-card">
  <Icon
    data={faLightbulb}
    scale={1}
    style={`border-radius: 4rem; color: #ffa029; padding: 0 1rem 0 .5rem; cursor: pointer; align-self: center`}
  />
  <p>{idea}</p>
</div>

<style>
  .idea-card {
    display: flex;
    padding: 1rem 1rem 1rem .5rem;
    background-color: #454545;
    border-radius: .2rem;
    align-items: center;
  }

  p {
    margin: 0;
    color: #eee;
    font-size: 0.75rem;
  }
</style>
