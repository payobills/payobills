<script lang="ts">
import { onMount } from "svelte";
import { tryLoadEnv } from "$lib/stores/env";
import { setupLiteServices } from "$lib/stores/lite-services";
import { nav } from "$lib/stores/nav";
import { CONSTANTS } from "../../constants";

onMount(async () => {
	// Try to load env Urls from localStorage
	await tryLoadEnv();
	// Load up Lite Services
	setupLiteServices();
	// Setup nav for Payobills Lite
	nav.update((prev) => ({ ...prev, title: CONSTANTS.PAYOBILLS_LITE }));
});
</script>

  <slot />

