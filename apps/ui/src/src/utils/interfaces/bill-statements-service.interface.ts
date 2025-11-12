import type { AddBillStatementDTO, Query } from "$lib/types";
import type { Writable } from "svelte/store";

export interface IBillStatementsService {
    queryBillStatementsByBillIds(billIds: string[]): Writable<Query<any[] | undefined>>
    addBillStatement(_: AddBillStatementDTO): Promise<unknown>;
}
