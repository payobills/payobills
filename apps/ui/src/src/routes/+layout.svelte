<script lang="ts">
    import { goto } from "$app/navigation";
  import Nav from "$lib/nav.svelte";
  import Keycloak from "keycloak-js";
  import { onMount } from "svelte";

  onMount(() => {
    const keycloakInstance = new Keycloak({
      // url: "keycloak.svc.playground.cluster.local",
      url: "http://localhost:8080",
      realm: "homelab",
      clientId: "payobills",
    });

    keycloakInstance
      .init({ onLoad: "login-required", checkLoginIframe: false })
      .then(() => {})
      .catch((err) => {
        // TODO: Go to an error page
      });
  });
</script>

<Nav />
<main>
  <slot />
</main>

<style>
  @import url("https://fonts.googleapis.com/css2?family=Work+Sans:wght@200;400;900&display=swap");
  @import url("https://fonts.googleapis.com/css2?family=Cutive+Mono&display=swap");

  :root {
    --color: #9f9f9f;
  }

  :global(body, html) {
    margin: 0;
    padding: 0;
  }
  :global(*) {
    font-family: "Work Sans", sans-serif;
  }

  :global(body) {
    height: 100vh;
  }

  :global(button) {
    /* align-self: flex-end; */
    border: none;
    border-radius: 2rem;
    background: #5b81bb;
    color: white;
    text-transform: uppercase;
    /* margin: 1rem 0 0 0; */
    padding: 1rem;
  }

  main {
    margin: 1rem;
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }
</style>
