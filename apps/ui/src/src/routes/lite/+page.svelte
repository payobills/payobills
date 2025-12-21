<script lang='ts'>
  import IconButton from "$lib/icon-button.svelte";
  import { faBell, faChartSimple, faListOl } from "@fortawesome/free-solid-svg-icons";

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
  <h1>Payobills Lite</h1>
  <div class="content">
  <h2>bills be gone</h2>
  <p class="one-liner">
    <span>payobills lite</span> is your go to app to be on top of your billing and transaction needs, all without sharing your data with third party services. Bring and keep your own data.
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

  <button on:click={() => {}}>Coming Soon</button>
  <p class='one-liner'>* Payobills is an open source project maintained by <span>Sahu, S</span> in his personal time. ❤️ is appreciated</p>
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
    margin: 1rem 0 .75rem 0;
    text-align: center;
  }

  span {
    font-weight: 800;
  }
  p,
  h2,
  h3 {
    color: #a0a0a8;
  }


  h2 {
    font-size: 2.5rem;
    font-weight: 900;
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
    width: 100%;
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
    font-family: 'Figtree';
    font-weight: 900;
    text-transform: unset;
    background-color: #2a276a;
    border: none;
    color: white;
    margin-top: 2rem;
    padding: 1rem 3.2rem;
    font-size: 1rem;
    border-radius: 2rem;
    cursor: pointer;
    transition: background 0.3s ease;
  }
</style>
