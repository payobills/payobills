<script lang="ts">
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

const _addBill = async () => {
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
