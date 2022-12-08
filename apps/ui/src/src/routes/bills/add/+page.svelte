<script lang="ts">
  import Card from "$lib/card.svelte";
  import { faCancel, faEllipsis } from "@fortawesome/free-solid-svg-icons";
  import {urql} from '$lib/stores/urql'
  import IconButton from "$lib/icon-button.svelte";
  import {
    queryStore,
    gql,
    getContextClient,
    mutationStore,
  } from "@urql/svelte";

  import { goto, afterNavigate } from "$app/navigation";
  // import { base } from "$app/paths";

  // let previousPage: string = base;

  // afterNavigate(({ from }) => {
  //   previousPage = from?.url.pathname || "/";
  // });

  let billingDate: number, payByDate: number, name: string;

  const addBill = () => {
    console.log('ok')
    let client;
    try{
      console.log('got client')
      $urql
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
        { name, payByDate, billingDate }
      )
      .toPromise()
      .then(() => {
        goto("/");
      });
    }
    catch(error) {
      console.log('coudln\'t get client')
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
        goto('/');
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
