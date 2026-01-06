<script lang="ts">
import { onMount } from "svelte";
import { goto } from "$app/navigation";
import { auth } from "$lib/stores/auth";
import { CONSTANTS } from "../../constants";

onMount(() => {
	localStorage.removeItem(CONSTANTS.REFRESH_TOKEN_KEY);
	localStorage.removeItem(CONSTANTS.REFRESH_TOKEN_EXPIRY_KEY);

	auth.set({
		refreshToken: null,
		refreshTokenExpiry: null,
	});

	goto("/");
});
</script>

<div>
  <p>Logging out...</p>
</div>
