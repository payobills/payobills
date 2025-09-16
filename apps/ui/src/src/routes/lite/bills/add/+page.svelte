<script lang="ts">
  import Card from "$lib/card.svelte";
  import { faCancel, faEllipsis } from "@fortawesome/free-solid-svg-icons";
  import { getLiteUrql } from "$lib/stores/urql--lite";
  import IconButton from "$lib/icon-button.svelte";
  import {
    queryStore,
    gql,
    getContextClient,
    mutationStore,
    Client,
  } from "@urql/svelte";
  import { goto, afterNavigate } from "$app/navigation";
  import { onMount } from "svelte";
  import { readable, type Readable } from "svelte/store";

  let billingDate: number, payByDate: number, name: string;
  let liteUrql: Readable<Client> | undefined;

  onMount(() => {
    liteUrql = readable(getLiteUrql());
  });

  const addBill = () => {
    let client;
    try {
      $liteUrql
        ?.mutation(
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
          { name, payByDate, billingDate }
        )
        .toPromise()
        .then(() => {
          // goto("/");
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
