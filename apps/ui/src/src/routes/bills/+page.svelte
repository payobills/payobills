<script lang="ts">
    import { queryStore, gql } from "@urql/svelte";
    import { urql } from "$lib/stores/urql";
    import { page } from "$app/stores";
    import PaymentTimelinePill from "$lib/payment-timeline-pill.svelte";
    import Nav from "$lib/nav.svelte";
    import Card from "$lib/card.svelte";

    let billId: any;
    let billByIdQuery: any;
    let refreshKey: number = Date.now();

    billId = $page.url.searchParams.get("id");
    $: billByIdQuery = queryStore({
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
        variables: { billId, refreshKey },
    });

    async function markPaid() {
        const markPaidQuery = $urql
            .mutation(
                gql`
                    mutation markPayment($billId: UUID!) {
                        markPayment(dto: { id: $billId }) {
                            id
                            createdAt
                            updatedAt
                            billingPeriod {
                                start
                                end
                            }
                        }
                    }
                `,
                { billId }
            )
            .toPromise();

        let { error } = await markPaidQuery;

        if (error) {
            console.log(error);
            return;
        }

        refreshKey = Date.now();
    }
</script>

<Nav />
<Card>
    <div class="content">
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
                            }).format(
                                new Date(payment.createdAt).getTime()
                            )}</span
                        >
                    </p>
                {/each}
            {/if}
        {/if}

        <div class="actions">
            <button class="markPaid" on:click={async () => await markPaid()}
                >mark as paid</button
            >
        </div>
    </div>
</Card>

<style>
    .content {
        display: flex;
        flex-direction: column;
        flex-grow: 1;
    }
    .actions {
        display: flex;
        flex-grow: 1;
        justify-content: flex-end;
    }

    .markPaid {
        align-self: flex-end;
    }
</style>
