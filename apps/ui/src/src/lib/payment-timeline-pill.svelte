<script lang="ts">
    export let item: any;
    export let showLabel = true;
</script>

<div class="item">

    <div class="pill">
        {#if item.billingDate > item.payByDate}
            <span
                data-start={0}
                style={`flex-grow: ${item.payByDate}`}
                class="pill--filled"
            />
            <span
                data-start={item.payByDate}
                style={`flex-grow: ${item.billingDate - item.payByDate + 1}`}
                class="pill--empty"
            />
            <span
                data-start={item.billingDate}
                style={`flex-grow: ${31 - item.billingDate + 1}`}
                class="pill--filled"
            />
        {:else}
            <span
                data-start={0}
                style={`flex-grow: ${item.billingDate}`}
                class="pill--empty"
            />
            <span
                data-start={item.billingDate}
                style={`flex-grow: ${item.payByDate}`}
                class="pill--filled"
            />
            <span
                data-start={item.payByDate}
                style={`flex-grow: ${31 - item.billingDate + 1}`}
                class="pill--empty"
            />
        {/if}
    </div>
    {#if showLabel}
    <span data-start={0} data-length={item.payByDate} class="item-title"
        >{item.name}</span
    >
    {/if}
</div>

<style>
    .pill {
        display: flex;
        width: 100%;
        padding: .25rem 0 .25rem 0;
        margin: 0;
    }
    .pill > span {
        height: 0.3125rem;
        border-radius: 0.25rem;
        margin: 1.25rem 0 0 0;
    }
    .pill--filled {
        background-color: #7a98c5;
    }
    .pill--empty {
        background-color: #e5ecf8;
    }
    .item-title {
        font-size: 0.75rem;
        margin: 2rem 0;
        color: var(--primary-color);
    }
</style>
