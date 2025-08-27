<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import { CONSTANTS } from "../constants";
  import { auth, loadAuthFromLocalStorage } from "$lib/stores/auth";
  import { get } from "svelte/store";

  /** @type {import('./$types').PageData} */
  export let data: any;

  // async function login() {
  //     // await goto(data.urls.loginUrl);
  //     await goto("/timeline");
  // }

  onMount(async () => {
    // loadAuthFromLocalStorage();
    // const authState = get(auth);
    // if (authState !== null)
    const rootRoute = data.rootRoute
    await goto(rootRoute);
  });
</script>

<div class="main">
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
    font-weight: 600;
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
