import type { AddBillDTO, BillDTO, Query } from "$lib/types";
import type { Writable } from "svelte/store";

export interface IBillsService {
    queryBills(): Writable<Query<BillDTO[] | undefined>>
    addBill(bill: AddBillDTO): Promise<unknown>;
}
