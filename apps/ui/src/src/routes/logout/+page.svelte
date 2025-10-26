<script lang="ts">
  import { goto } from "$app/navigation";
  import { redirect } from "@sveltejs/kit";
  import { CONSTANTS } from "../../constants";
  import { auth } from "$lib/stores/auth";
  import { envStore, type EnvStore } from "$lib/stores/env";
  import { onMount } from "svelte";

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
