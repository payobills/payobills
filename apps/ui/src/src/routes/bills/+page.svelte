<script lang="ts">
    import { queryStore, gql } from "@urql/svelte";
    import { urql } from "$lib/stores/urql";
    import { page } from "$app/stores";
    import PaymentTimelinePill from "$lib/payment-timeline-pill.svelte";
    import Nav from "$lib/nav.svelte";
    import { dataset_dev } from "svelte/internal";

    const billId = $page.url.searchParams.get("id");
    const billByIdQuery = queryStore({
        client: $urql,
        query: gql`
            query billById($billId: UUID!) {
                billById(id: $billId) {
                    id
                    name
                    billingDate
                    payByDate
                    updatedAt
                    createdAt
                    payments {
                        id
                        amount
                        billingPeriod {
                            start
                            end
                        }
                        createdAt
                        updatedAt
                    }
                }
            }
        `,
        variables: { billId },
    });
</script>

<Nav />
<div>
    {#if $billByIdQuery.fetching}
        <p>Loading...</p>
    {:else if $billByIdQuery.error}
        <p>🙆‍♂️ Uh oh! Unable to fetch your bill!</p>
    {:else}
        <h1>{$billByIdQuery.data.billById.name}</h1>
        <PaymentTimelinePill
            item={$billByIdQuery.data.billById}
            showLabel={false}
        />
        {#if $billByIdQuery.data.billById.payments.length == 0}
            <p>we don't see any payments made for this bill. 😞</p>
        {:else}
            <h2>payments</h2>
            {#each $billByIdQuery.data.billById.payments as payment}
                <p>
                    {#if payment.amount}
                        <span class="amount">{payment.amount}</span>
                    {:else}
                        <span class="amount--unknown">Unknown Amount </span>
                    {/if}
                    <span>•</span>
                    <span class="paid-on"
                        >{Intl.DateTimeFormat(undefined, {
                            month: "long",
                        }).format(new Date(payment.createdAt).getTime())}</span
                    >
                </p>
            {/each}
        {/if}
    {/if}
</div>

<style>
    div {
        display: flex;
        flex-direction: column;
        flex-grow: 1;
        background-color: #f3f3f3;
        padding: 1rem 1rem;
        border-radius: 2rem;
    }
</style>
