<script lang="ts">
    import { goto } from "$app/navigation";
    
    /** @type {import('./$types').PageData} */
	export let data: any;

    async function login() {
        await goto(data.urls.loginUrl);
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
                src={`${data.urls.oidcUrl}/auth/welcome-content/keycloak-project.png`}
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
