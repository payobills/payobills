import type { Writable } from "svelte/store";
import type { AddBillDTO, BillDTO, Query } from "$lib/types";

export interface IBillsService {
	queryBills(): Writable<Query<BillDTO[] | undefined>>;
	queryBillById(id: string): Writable<Query<BillDTO | undefined>>;
	addBill(bill: AddBillDTO): Promise<unknown>;
	updateBill(id: string, bill: AddBillDTO): Promise<unknown>;
}
