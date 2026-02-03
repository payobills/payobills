<script lang="ts">
  import IconButton from "$lib/icon-button.svelte";
  import {
    faBell,
    faChartSimple,
    faListOl,
  } from "@fortawesome/free-solid-svg-icons";

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
  <div class="content">
    <h1>Payobills <span>Lite</span></h1>
    <p class="one-liner">
      <span>payobills lite</span> is your go-to app to be on top of your billing
      and transaction needs, all without sharing your data with third party services.
      Bring and keep your own data.
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
          Track all your bills together in a single place. No need to do it
          across different apps.
        </p>
      </div>
    </div>

    <div class="card lg:card-side bg-base-300 shadow-lg">
      <IconButton
        icon={faBell}
        scale={0.75}
        style={iconStyle(["#61C1C7", "#518EE5"])}
      />
      <div class="">
        <h3>Reminders</h3>
        <p>
          Get notified about when to pay your bills, so you never miss a due
          date.
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
          See where your money goes with a clear, categorized view of your
          monthly spending.
        </p>
      </div>
    </div>


  <blockquote class="alert not-italic items-start text-xs leading-loose *:m-0!">
    <p class="one-liner">
      * Payobills is an open source project maintained by <span>Sahu, S</span>
      in his personal time ❤️ is appreciated
    </p>
  </blockquote>

  <button on:click={() => {}}>Coming Soon</button>

  </div>
</section>

<style>
  :global(main) {
    align-items: center;
    justify-content: stretch;
  }

  section {
    padding: 1rem;
    align-items: stretch;
    display: flex;
    flex-direction: column;
    max-width: 60rem;
    flex-grow: 1;
  }

  .content {
    justify-content: center;
    flex-grow: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  p {
    font-size: 1rem;
    margin-bottom: 0;
  }

  .one-liner {
    margin: 1rem 0 0.75rem 0;
    text-align: center;
  }

  span {
    font-weight: 800;
  }

  h1 {
    font-size: 3.5rem;
    font-weight: 800;
  }

  h1 > span {
    font-weight: 100;
  }

  p,
  h3 {
    color: #a0a0a8;
  }

  .card {
    display: flex;
    align-items: center;
    min-height: 8rem;
    padding: 2rem 2.5rem;
    margin-bottom: 1rem;
    width: 100%;
    gap: 2rem;
  }

  h3 {
    font-size: 1.25rem;
    font-weight: 900;
    color: white;
    margin: 0;
  }

  button {
    align-self: center;
    width: auto;
    font-family: "Figtree";
    font-weight: 900;
    text-transform: unset;
    background: linear-gradient(to right, darkgray, gray);
    border: none;
    color: white;
    margin: 1rem 0;
    margin-top: 4rem;
    padding: 1rem 10rem;
    font-size: 1.5rem;
    text-align: center;
    border-radius: 4rem;
    cursor: not-allowed;
    transition: background 0.3s ease;
  }
</style>
