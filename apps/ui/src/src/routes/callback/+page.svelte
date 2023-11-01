<script lang="ts">
    import { goto } from "$app/navigation";
    import { redirect } from "@sveltejs/kit";
    import { onMount } from "svelte";

    let accessToken: string | null = null;

    onMount(async () => {
        let params = new URLSearchParams(window.location.search);
        let code = params.get("code");

        if (code === null) throw redirect(301, "/login");

        let encodedParams = new URLSearchParams({
            client_id: "payobills",
            redirect_uri: "http://localhost:3000/callback",
            grant_type: "authorization_code",
            scope: "openid",
            code: code || "",
        });

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

        await goto("/timeline");
    });
</script>
