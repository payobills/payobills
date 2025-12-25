<script lang="ts">
  import Card from "$lib/card.svelte";
  import { faCancel } from "@fortawesome/free-solid-svg-icons";
  import IconButton from "$lib/icon-button.svelte";
  import { goto } from "$app/navigation";
  import { liteServices } from "../../../../lib/stores/lite-services";

  $: billsService = $liteServices?.billsService

  let billingDate: number, payByDate: number, name: string;

  const addBill = () => {
    try {
      billsService.addBill({ name, payByDate, billingDate })
      .then(() => {
        goto("/");
      });
    }
    catch(error) {
      console.error("couldn't get client", error);
    }
  };
</script>

<Card title="add bill">
  <div class="add-bill-form">
    <div>name</div>
    <input type="text" placeholder="My Credit Card" bind:value={name} />
    <div>billing date</div>
    <input type="number" placeholder="1" bind:value={billingDate} />
    <div>pay by date</div>
    <input type="number" placeholder="12" bind:value={payByDate} />
  </div>
  <div class="actions">
    <IconButton
      icon={faCancel}
      backgroundColor="#d96c59"
      rounded={true}
      on:click={() => {
        window.history.back();
      }}
    />

    <button on:click={addBill}>save</button>
  </div>
</Card>

<style>
  div {
    font-size: 0.8rem;
    color: var(--color);
  }

  input {
    border: none;
    margin: 0.5rem 0;
    padding: 0.5rem;
    align-self: stretch;
  }

  .add-bill-form {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }

  .actions {
    display: flex;
    justify-content: space-between;
    align-self: flex;
  }
</style>
