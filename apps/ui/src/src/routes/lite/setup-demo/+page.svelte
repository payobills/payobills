<script lang="ts">
import { onMount } from "svelte";
import { setupLiteDemoAsync } from "$lib/stores/lite-demo-store";

let _isSettingUp = true;

onMount(async () => {
	await setupLiteDemoAsync();
	_isSettingUp = false;
});
</script>

<div>
  {#if isSettingUp}
 setting up demo environment...
{:else} 
<div>setup finished</div>
    <button on:click={() => { window.history.back() }}>explore the demo</button>
{/if}
</div>
