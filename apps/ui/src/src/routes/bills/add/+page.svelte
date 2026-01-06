<script lang="ts">
import { faCancel, faEllipsis } from "@fortawesome/free-solid-svg-icons";
import { getContextClient, gql, mutationStore, queryStore } from "@urql/svelte";
import { onMount } from "svelte";
import { afterNavigate, goto } from "$app/navigation";
import Card from "$lib/card.svelte";
import IconButton from "$lib/icon-button.svelte";
import { nav } from "$lib/stores/nav";
import { billsUrql } from "$lib/stores/urql";

// import { base } from "$app/paths";

// let previousPage: string = base;

// afterNavigate(({ from }) => {
//   previousPage = from?.url.pathname || "/";
// });

let billingDate: number, payByDate: number, name: string;

onMount(() => {
	nav.update((prev) => ({ ...prev, isOpen: true }));
});

const addBill = () => {
	let client;
	try {
		$billsUrql
			.mutation(
				gql`
          mutation ($name: String!, $payByDate: Int!, $billingDate: Int!) {
            addBill(
              billDto: {
                name: $name
                payByDate: $payByDate
                billingDate: $billingDate
                latePayByDate: 0
              }
            ) {
              name
              payByDate
              latePayByDate
              billingDate
              createdAt
              updatedAt
              id
            }
          }
        `,
				{ name, payByDate, billingDate },
			)
			.toPromise()
			.then(() => {
				goto("/");
			});
	} catch (error) {
		console.error("couldn't get client", error);
	}
};
</script>

<Card title="add bill">
  <!-- <div> -->
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
    <!-- {#if !isAddingBill} -->
    <button on:click={addBill}>save</button>
    <!-- {:else}
      <IconButton icon={faEllipsis} backgroundColor={'white'} rounded={false} />
    {/if} -->
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
