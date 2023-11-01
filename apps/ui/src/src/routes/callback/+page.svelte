<script lang="ts">
    import { redirect } from "@sveltejs/kit";
    import { onMount } from "svelte";

    let accessToken: string | null = null;

    onMount(async () => {
        let params = new URLSearchParams(window.location.search);
        let code = params.get("code");

        if (code === null) throw redirect(301, "/login");

        let encodedParams = new URLSearchParams();
        encodedParams.append("client_id", "payobills");
        encodedParams.append(
            "redirect_uri",
            "http://localhost:3000/callback"
        );
        encodedParams.append("grant_type", "authorization_code");
        encodedParams.append("scope", "openid");
        encodedParams.append("code", code || "");

        let accessTokenResponse = await fetch(
            "http://localhost:3001/auth/realms/homelab/protocol/openid-connect/token",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    Accept: "application/json",
                },
                body: encodedParams,
            }
        );

        let accessTokenResponseJSON = await accessTokenResponse.json();
        accessToken = accessTokenResponseJSON["access_token"];
    });
</script>

<p>{accessToken}</p>
