import type { Query, TransactionDTO } from "$lib/types";
import type { Writable } from "svelte/store";

export interface ITransactionsService {
    queryTransactionsForCurrentMonth(): Writable<Query<TransactionDTO[] | undefined>>
}
