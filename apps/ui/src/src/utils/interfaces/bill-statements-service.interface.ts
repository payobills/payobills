import type { Writable } from "svelte/store";
import type { AddBillStatementDTO, BillStatementDTO, Query } from "$lib/types";

export interface IBillStatementsService {
	queryBillStatementsByBillIds(
		billIds: string[],
	): Writable<Query<{ [key: string]: BillStatementDTO[] } | undefined>>;
	addBillStatement(_: AddBillStatementDTO): Promise<unknown>;
}
