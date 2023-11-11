<script lang="ts">
    import { queryStore, gql } from "@urql/svelte";
    import { urql } from "$lib/stores/urql";
    import { page } from "$app/stores";
    import PaymentTimelinePill from "$lib/payment-timeline-pill.svelte";
    import Nav from "$lib/nav.svelte";

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
                userId
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
    <PaymentTimelinePill item={$billByIdQuery.data.billById} showLabel={false}/>
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
