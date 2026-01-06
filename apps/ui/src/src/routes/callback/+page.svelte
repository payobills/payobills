<script lang="ts">
import { redirect } from "@sveltejs/kit";
import { goto } from "$app/navigation";
import { auth } from "$lib/stores/auth";
import { type EnvStore, envStore } from "$lib/stores/env";
import { CONSTANTS } from "../../constants";

let accessToken: string | null = null;

const checkLogin = async (env: EnvStore | null) => {
	if (env === null) return;

	const envStore = env as EnvStore;

	const params = new URLSearchParams(window.location.search);
	const code = params.get("code");

	if (code === null) throw redirect(301, "/login");

	const encodedParams = new URLSearchParams({
		client_id: envStore.INJECTED_OIDC_CLIENT_ID || "",
		redirect_uri: `${envStore.INJECTED_OWN_URL}/callback`,
		grant_type: "authorization_code",
		scope: "openid",
		code: code || "",
	});

	const tokenUrl = `${envStore?.INJECTED_OIDC_TENANT_URL}/realms/Homelab-SBX/protocol/openid-connect/token`;

	const accessTokenResponse = await fetch(tokenUrl, {
		method: "POST",
		headers: {
			"Content-Type": "application/x-www-form-urlencoded",
			Accept: "application/json",
		},
		body: encodedParams,
	});

	const accessTokenResponseJSON = await accessTokenResponse.json();
	accessToken = accessTokenResponseJSON["access_token"];
	const refreshToken = accessTokenResponseJSON["refresh_token"];

	var refreshTokenPayload = refreshToken.split(".")[1];
	var refreshTokenPayloadObject = JSON.parse(window.atob(refreshTokenPayload));
	var refreshTokenExpiry = refreshTokenPayloadObject.exp;

	localStorage.setItem(CONSTANTS.REFRESH_TOKEN_KEY, refreshToken);
	localStorage.setItem(
		CONSTANTS.REFRESH_TOKEN_EXPIRY_KEY,
		(refreshTokenExpiry * 1000).toString(),
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
