<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import { auth, loadAuthFromLocalStorage } from "$lib/stores/auth";
  import { envStore } from "$lib/stores/env";
  import { get } from "svelte/store";
  import IconButton from "$lib/icon-button.svelte";
  import {
    faBell,
    faChartSimple,
    faListOl,
  } from "@fortawesome/free-solid-svg-icons";
  import { nav } from "$lib/stores/nav";
  import { getIdpBaseUrl } from "$utils/auth";

  onMount(async () => {
    nav.update(prev => ({...prev, isOpen: false }));
    loadAuthFromLocalStorage();

    const authState = get(auth);
    // console.log("Auth State on mount:", authState);
    if (authState?.refreshToken) await goto("/timeline");
  });

  const iconStyle = (colorsLeftToRight: string[] = []) => `
    background: linear-gradient(to right, ${colorsLeftToRight.join(", ")});
    border: none;
    color: white;
    margin-right: 2rem;
    border-radius: 2.5rem;
    height: 5rem;
    width: 5rem;
    cursor: pointer;
  `;
</script>

<section>
  <h1>Payobills</h1>
  
  <p class="one-liner">
    <span>payobills</span> is your go to app for all billing and transaction needs,
    all without sharing your data with third parties
  </p>

  <div class="card lg:card-side bg-base-300 shadow-lg">
    <IconButton
      icon={faChartSimple}
      scale={0.75}
      style={iconStyle(["#C850A3", "#8341D7"])}
    />
    <div>
      <h3>Bill Management</h3>
      <p>
        Track all your bills together in a single place. No need to do it across
        different apps.
      </p>
    </div>
  </div>

  <div class="card lg:card-side bg-base-300 shadow-lg">
    <IconButton
      icon={faBell}
      scale={0.75}
      style={iconStyle(["#61C1C7", "#518EE5"])}
    />
    <div>
      <h3>Reminders</h3>
      <p>
        Get notified about when to pay your bills, so you never miss a due date.
      </p>
    </div>
  </div>

  <div class="card lg:card-side bg-base-300 shadow-lg">
    <IconButton
      icon={faListOl}
      scale={0.75}
      style={iconStyle(["#F0B347", "#E98739"])}
    />
    <div>
      <h3>Detailed Transaction History</h3>
      <p>
        See where your money goes with a clear, categorized view of your monthly
        spending.
      </p>
    </div>
  </div>



  <blockquote class="alert not-italic items-start text-xs leading-loose *:m-0!">
  <p>
    Payobills is an open source project maintained by <span>Sahu, S</span> in his
    personal time. ❤️ is appreciated
  </p>
  </blockquote>

    <a
    class="login-link"
    href={$envStore?.INJECTED_OIDC_TENANT_LOGIN_URL_TEMPLATE
      ?.replace("${INJECTED_OWN_URL}", $envStore?.INJECTED_OWN_URL)
      ?.replace("${INJECTED_OIDC_CLIENT_ID}", $envStore?.INJECTED_OIDC_CLIENT_ID)
      ?.replace("${INJECTED_OIDC_TENANT_URL}",
        getIdpBaseUrl(
          $envStore?.INJECTED_OWN_URL,
          $envStore?.INJECTED_OIDC_TENANT_URL
        )
      )}>Login</a
  >
</section>

<style>
  section {
    padding: 1rem;
    align-items: stretch;
    align-self: center;
    display: flex;
    flex-direction: column;
    max-width: 60rem;
  }

  .one-liner {
    margin-bottom: 2rem;
    text-align: center;
    font-size: 1.5rem;
  }

  span {
    font-weight: 800;
  }

  p,
  h2,
  h3 {
    color: #a0a0a8;
  }

  h1 {
    font-size: 3.5rem;
    font-weight: 900;
    text-align: center;
    margin: 1rem;
    margin-bottom: 4rem;
  }

  .card {
    display: flex;
    align-items: center;
    min-height: 8rem;
    padding: 2rem 2.5rem;
    margin-bottom: 1rem;
    gap: 2rem;
  }

  h3 {
    font-size: 1.25rem;
    font-weight: 900;
    color: white;
    margin: 0;
  }

  .login-link {
    align-self: center;
    width: auto;
    font-family: "Figtree";
    font-weight: 900;
    text-transform: unset;
    background: linear-gradient(to right, var(--color-primary), #633cbd);
    border: none;
    color: white;
    margin: 1rem 0;
    margin-top: 4rem;
    padding: 1rem 10rem;
    font-size: 1.5rem;
    text-align: center;
    border-radius: 4rem;
    cursor: pointer;
    transition: background 0.3s ease;
  }
</style>
