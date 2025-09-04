<script lang="ts">
    import { goto } from "$app/navigation";
    import { redirect } from "@sveltejs/kit";
    import { onMount } from "svelte";
    import { CONSTANTS } from "../../constants";
    import { auth } from "$lib/stores/auth";

    /** @type {import('./$types').PageData} */
    // export let data: any;

    let accessToken: string | null = null;

    onMount(async () => {
        let params = new URLSearchParams(window.location.search);
        let code = params.get("code");

        if (code === null) throw redirect(301, "/login");

        let encodedParams = new URLSearchParams({
            client_id: import.meta.env.PUBLIC_OIDC_CLIENT_ID,
            redirect_uri: `${import.meta.env.PUBLIC_OWN_URL}/callback`,
            grant_type: "authorization_code",
            scope: "openid",
            code: code || "",
        });

        let accessTokenResponse = await fetch(
            `${import.meta.env.PUBLIC_OIDC_URL}/auth/realms/homelab/protocol/openid-connect/token`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    Accept: "application/json",
                },
                body: encodedParams,
            },
        );

        let accessTokenResponseJSON = await accessTokenResponse.json();
        accessToken = accessTokenResponseJSON["access_token"];
        let refreshToken = accessTokenResponseJSON["refresh_token"];

        var refreshTokenPayload = refreshToken.split(".")[1];
        var refreshTokenPayloadObject = JSON.parse(
            window.atob(refreshTokenPayload),
        );
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
    });
</script>
