<script lang="ts">
import Card from "$lib/card.svelte";
import { faCancel } from "@fortawesome/free-solid-svg-icons";
import IconButton from "$lib/icon-button.svelte";
import { liteServices } from "$lib/stores/lite-services";
import { onMount } from "svelte";
import { nav } from "$lib/stores/nav";
import { Crud } from "$lib/types";

$: billsService = $liteServices?.billsService;

export let existingBillId: string | null = null;

let billingDate: number, payByDate: number, name: string;
$: existingBillQuery =
  existingBillId && billsService
    ? billsService.queryBillById(existingBillId)
    : null;
$: mode = existingBillId ? Crud.Update : Crud.Create;

$: {
  // Bill finished loading without errors
  if (!$existingBillQuery?.fetching && !$existingBillQuery?.error) {
    billingDate = $existingBillQuery?.data.billingDate;
    payByDate = $existingBillQuery?.data.payByDate;
    name = $existingBillQuery?.data.name;
  }
}

onMount(() => {
  nav.update((prev) => ({ ...prev, isOpen: true }));
  existingBillId =
    new URLSearchParams(window.location.search).get("existing-bill-id") ?? null;
});

const addBill = async () => {
  try {
    const savePromise = existingBillId
      ? billsService.updateBill(existingBillId, {
          name,
          payByDate,
          billingDate,
          id: existingBillId,
        })
      : billsService.addBill({ name, payByDate, billingDate });

    await savePromise;
    history.back();
  } catch (error) {
    console.error("Unable to save/update the bill", error);
  }
};
</script>

{#if existingBillId && (!existingBillQuery || $existingBillQuery?.fetching)}
<div>loading your bill...</div>
{:else if $existingBillQuery?.error}
<div>looks like there was an issue loading your bill</div>
{:else}

<Card title={mode == Crud.Create ? "add bill" : "update bill"}>
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
      on:click={() => { window.history.back(); }}
    />

    <button on:click={addBill}>save</button>
  </div>
</Card>
{/if}

<style>
  div {
    font-size: 0.75rem;
    color: var(--color-neutral-content, #8892a4);
    font-family: "Syne", sans-serif;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    font-weight: 600;
    margin-top: 0.75rem;
  }

  input {
    background-color: var(--color-base-300, #1c1c26);
    border: 1px solid #2a2a38;
    border-radius: 0.375rem;
    color: var(--color-base-content, #dde1eb);
    margin: 0.25rem 0 0;
    padding: 0.625rem 0.75rem;
    font-size: 0.875rem;
    font-family: "DM Sans", sans-serif;
    align-self: stretch;
    transition: border-color 0.15s ease;
    width: 100%;
  }

  input:focus {
    outline: none;
    border-color: var(--color-primary, #00d4b8);
  }

  input::placeholder {
    color: #3a3a50;
  }

  .add-bill-form {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }

  .actions {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 1.25rem;
  }

  .actions button {
    background-color: var(--color-primary, #00d4b8);
    color: var(--color-primary-content, #000a09);
    border: none;
    border-radius: 0.375rem;
    padding: 0.625rem 1.5rem;
    font-family: "Syne", sans-serif;
    font-size: 0.8125rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    cursor: pointer;
    transition: opacity 0.15s ease;
  }

  .actions button:hover {
    opacity: 0.85;
  }
</style>
