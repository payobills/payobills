<script lang="ts">
  import Icon from "svelte-awesome";
  import { faLightbulb } from "@fortawesome/free-solid-svg-icons";

  export let idea: string;
</script>

<div class="idea-card">
  <Icon
    data={faLightbulb}
    scale={1.5}
    style={`border-radius: 4rem; color: #ffa029; padding: 0 1rem 0 .5rem; cursor: pointer; align-self: center`}
  />
  <p>{idea}</p>
</div>

<style>
  .idea-card {
    display: flex;
    margin: 1rem 0;
    padding: 0.5rem 1rem;
    background-color: #454545;
    border-radius: .2rem;
  }

  p {
    margin: 0;
    color: #eee;
    font-size: 0.75rem;
  }
</style>
