<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import { CONSTANTS } from "../constants";
  import { auth, loadAuthFromLocalStorage } from "$lib/stores/auth";
  import { envStore } from "$lib/stores/env";
  import { get } from "svelte/store";

  onMount(async () => {
    loadAuthFromLocalStorage();

    const authState = get(auth);
    // console.log("Auth State on mount:", authState);
    if (authState?.refreshToken) await goto("/timeline");
  });
</script>

<div class="main">
  <a
    href={$envStore?.INJECTED_OIDC_TENANT_LOGIN_URL_TEMPLATE.replace(
      "${INJECTED_OWN_URL}",
      $envStore?.INJECTED_OWN_URL
    )
      .replace("${INJECTED_OIDC_CLIENT_ID}", $envStore?.INJECTED_OIDC_CLIENT_ID)
      .replace(
        "${INJECTED_OIDC_TENANT_URL}",
        $envStore?.INJECTED_OIDC_TENANT_URL
      )}>Login</a
  >
</div>

<style>
  .main {
    align-self: flex-start;
    display: flex;
    flex-direction: column;
    height: 100%;
    width: 100%;
  }

  .intro {
    flex-grow: 2;
    display: flex;
    flex-direction: column;
    justify-content: center;
  }

  .cta {
    flex-grow: 1;
    display: flex;
    justify-content: center;
    align-items: center;
  }

  img {
    height: 2rem;
    filter: grayscale();
  }
  span {
    margin-right: 0.5rem;
    font-weight: 600;
  }

  button {
    width: auto;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #202020;
  }

  h1 {
    font-size: 4rem;
    text-align: center;
  }

  p {
    text-align: center;
  }
</style>
