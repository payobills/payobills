<script lang="ts">
    import Icon from "svelte-awesome/components/Icon.svelte";
    import type { IconDefinition } from "@fortawesome/free-solid-svg-icons";
    import { faLightbulb } from "@fortawesome/free-solid-svg-icons";

    export let idea: string;
</script>

<div class='idea-card'>
<Icon
    data={faLightbulb}
    scale={1.5}
    style={`border-radius: 4rem; color: #ffa029; padding: 0 1rem; cursor: pointer; align-self: center`}
/>
<p>{idea}</p>
</div>

<style>
.idea-card {
    display: flex;
    margin: 1rem 0;
    padding: 0.5rem 1rem;
    background-color: #5b81bb;
    border-radius: 1rem;
}

p {margin: 0; color: #eee;}
</style>