<script lang="ts">
    import { goto } from "$app/navigation";

    async function login() {
        await goto(
            "http://localhost:3001/auth/realms/homelab/protocol/openid-connect/auth?client_id=payobills&redirect_uri=http://localhost:3000/callback&response_type=code&grant_type=authorization_code&scope=openid"
        );
    }
</script>

<div class="main">
    <section class="intro">
        <h1>payobills</h1>
        <p>manage your finances like a pro</p>
    </section>
    <section class="cta">
        <button on:click={login}>
            <span>Login with SSO</span>
            <img
                src="http://localhost:3001/auth/welcome-content/keycloak-project.png"
                alt="Keycloak Logo"
            />
        </button>
    </section>
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
    }
</style>
