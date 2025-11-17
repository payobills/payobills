import type { AddBillStatementDTO, BillStatDTO, Query } from "$lib/types";
import type { Writable } from "svelte/store";

export interface IBillStatsService {
    queryBillStats(year: string, month: string): Writable<Query<BillStatDTO | undefined>>
}
