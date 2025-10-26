<script lang="ts">
  import { goto } from "$app/navigation";
  import { redirect } from "@sveltejs/kit";
  import { CONSTANTS } from "../../constants";
  import { auth } from "$lib/stores/auth";
  import { envStore, type EnvStore } from "$lib/stores/env";

  let accessToken: string | null = null;

  const checkLogin = async (env: EnvStore | null) => {
    if (env === null) return;

    let envStore = env as EnvStore;

    let params = new URLSearchParams(window.location.search);
    let code = params.get("code");

    if (code === null) throw redirect(301, "/login");

    let encodedParams = new URLSearchParams({
      client_id: envStore.INJECTED_OIDC_CLIENT_ID || "",
      redirect_uri: `${envStore.INJECTED_OWN_URL}/callback`,
      grant_type: "authorization_code",
      scope: "openid",
      code: code || "",
    });

    const tokenUrl = `${envStore?.INJECTED_OIDC_TENANT_URL}/realms/Homelab-SBX/protocol/openid-connect/token`;

    let accessTokenResponse = await fetch(tokenUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
        Accept: "application/json",
      },
      body: encodedParams,
    });

    let accessTokenResponseJSON = await accessTokenResponse.json();
    accessToken = accessTokenResponseJSON["access_token"];
    let refreshToken = accessTokenResponseJSON["refresh_token"];

    var refreshTokenPayload = refreshToken.split(".")[1];
    var refreshTokenPayloadObject = JSON.parse(
      window.atob(refreshTokenPayload)
    );
    var refreshTokenExpiry = refreshTokenPayloadObject.exp;

    localStorage.setItem(CONSTANTS.REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(
      CONSTANTS.REFRESH_TOKEN_EXPIRY_KEY,
      (refreshTokenExpiry * 1000).toString()
    );

    auth.set({
      refreshToken,
      refreshTokenExpiry,
    });

    await goto("/timeline");
  };

  $: {
    checkLogin($envStore);
  }
</script>

<div>
  <p>Logging in...</p>
</div>
