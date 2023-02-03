<script lang="ts">
    import { createEventDispatcher } from "svelte";
    import { addBill, markAsPaid, filterBills, billsStore } from "$lib/stores/bills";
    import type { Bill } from "$lib/stores/bills";
    import type { BillFilter } from "$lib/types";

    let filter: BillFilter = "all";

    let allFilterChecked = filter === "all"
    let paidFilterChecked = filter === "all"
    let unpaidFilterChecked = filter === "all"
    $: filteredBills = filterBills(filter, $billsStore);

    const handleFilterChange = (event: Event) => {
        filter = (event.target as HTMLInputElement).value as BillFilter;
    };

    const onSubmit = (e: any) => {
        const input = e.target.elements.bill;
console.log(input)
console.log(input.files)
        addBill({
            id: Date.now(),
            file: input.files[0],
            paid: false,
        });
        input.value = "";
    };
</script>

<h1>My Bills</h1>

<form on:submit|preventDefault={onSubmit}>
    <input type="file" name="bill" accept="application/pdf" />
    <button type="submit">Upload Bill</button>
</form>

<div>
    <label>
        <input
            type="radio"
            name="filter"
            value="all"
            bind:group={allFilterChecked}
            on:change={handleFilterChange}
        />
        Show All
    </label>
    <label>
        <input
            type="radio"
            name="filter"
            value="paid"
            bind:group={paidFilterChecked}
            on:change={handleFilterChange}
        />
        Show Paid
    </label>
    <label>
        <input
            type="radio"
            name="filter"
            value="unpaid"
            bind:group={unpaidFilterChecked}
            on:change={handleFilterChange}
        />
        Show Unpaid
    </label>
</div>

<ul>
    {#each filteredBills as bill}
        <li>
            <a href={URL.createObjectURL(bill.file)} target="_blank"
                >{bill.file.name}</a
            >
            {#if !bill.paid}
                <button on:click={() => markAsPaid(bill.id)}
                    >Mark as Paid</button
                >
            {/if}
        </li>
    {/each}
</ul>
