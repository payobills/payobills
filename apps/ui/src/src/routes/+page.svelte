<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import { auth, loadAuthFromLocalStorage } from "$lib/stores/auth";
  import { envStore } from "$lib/stores/env";
  import { get } from "svelte/store";
  import IconButton from "$lib/icon-button.svelte";
  import { faBell, faChartSimple, faListOl } from "@fortawesome/free-solid-svg-icons";
  import { nav } from "$lib/stores/nav";

  onMount(async () => {
    nav.set({ isOpen: false })
    loadAuthFromLocalStorage();

    const authState = get(auth);
    // console.log("Auth State on mount:", authState);
    if (authState?.refreshToken) await goto("/timeline");
  });


  const iconStyle = (colorsLeftToRight: string[] = []) => `
    background: linear-gradient(to right, ${colorsLeftToRight.join(', ')});
    border: none;
    color: white;
    padding: 1.6rem;
    margin-bottom: 1rem;
    border-radius: 2rem;
    cursor: pointer;
  `;
</script>

<section>
  <h1>Payobills</h1>
  <!-- <h2>bills be gone</h2> -->
  <p class="one-liner">
    <span>payobills</span> is your go to app for all billing and transaction
    needs, all without sharing your data with third parties
  </p>

  <div class="card">
    <IconButton icon={faChartSimple} scale={.75} style={iconStyle(['#C850A3', '#8341D7'])}/>
    <h3>Bill Management</h3>
    <p>
      Track all your bills together in a single place. No need to do it across different apps.
    </p>
  </div>

    <div class="card">
      <IconButton icon={faBell} scale={.75} style={iconStyle(['#61C1C7', '#518EE5'])}/>
    <h3>Reminders</h3>
    <p>
      Get notified about when to pay your bills, so you never miss a due date.
    </p>
  </div>

  <div class="card">
    <IconButton icon={faListOl} scale={.75} style={iconStyle(['#F0B347', '#E98739'])}/>
    <h3>Detailed Transaction History</h3>
    <p>
      See where your money goes with a clear, categorized view of your monthly
      spending.
    </p>
  </div>

  <a
    class="login-link"
    href={$envStore?.INJECTED_OIDC_TENANT_LOGIN_URL_TEMPLATE.replace(
      "${INJECTED_OWN_URL}",
      $envStore?.INJECTED_OWN_URL
    )
      .replace("${INJECTED_OIDC_CLIENT_ID}", $envStore?.INJECTED_OIDC_CLIENT_ID)
      .replace(
        "${INJECTED_OIDC_TENANT_URL}",
        $envStore?.INJECTED_OIDC_TENANT_URL
      )}>Login</a
  >

  <p class='one-liner'>Payobills is an open source project maintained by <span>Sahu, S</span> in his personal time. ❤️ is appreciated</p>
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
    color: var(--primary-accent-color);
    margin: 1rem;
    margin-bottom: 4rem;
  }



  h2 {
    font-size: 2.5rem;
    text-align: center;
    color: white;
    margin: 1rem;
  }

  .card {
    min-height: 8rem;
    background-color: rgb(59, 59, 59);
    padding: 2rem 2.5rem;
    margin-bottom: 1rem;
    border-radius: 2rem;
  }

  h3 {
    font-size: 1.25rem;
    font-weight: 900;
    color: white;
    margin: 0;
  }

  button, .login-link {
    align-self: center;
    width: auto;

    /* button with gradient from 696ADB to 633CBD from left to right */
    /* background:  */
    font-family: 'Figtree';
    font-weight: 900;
    text-transform: unset;
    background: linear-gradient(to right, #696adb, #633cbd);
    border: none;
    color: white;
    margin: 1rem 0;
    margin-top: 4rem;
    padding: 1rem 10rem;
    font-size: 1.5rem;
    border-radius: 1.5rem;
    cursor: pointer;
    transition: background 0.3s ease;
  }
</style>

