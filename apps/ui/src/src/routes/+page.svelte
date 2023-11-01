<script lang="ts">
    import { goto } from "$app/navigation";
    import { redirect } from "@sveltejs/kit";
    import { onMount } from "svelte";

    async function login() {
        await goto(
            "http://localhost:3001/auth/realms/homelab/protocol/openid-connect/auth?client_id=payobills&redirect_uri=http://localhost:3000/callback&response_type=code&grant_type=authorization_code&scope=openid"
        );
    }
</script>

<div>
    <button on:click={login}>
        <div>
            <span>Get started with Keycloak</span>
            <img
                src="http://localhost:3001/auth/welcome-content/keycloak-project.png"
                alt="Keycloak Logo"
            />
        </div>
    </button>
</div>

<style>
    div {
        display: flex;
        height: 100%;
        align-items: center;
        justify-content: center;
    }

    img {
        height: 2rem;
    }
    span {
        margin-right: .5rem;
    }

    button {
        width: auto;
    }
</style>
