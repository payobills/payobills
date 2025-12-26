import type { Response, Query, TransactionDTO, TransactionAddDTOInput } from "$lib/types";
import type { Writable } from "svelte/store";

export interface ITransactionsService {
    queryTransactions(input: { filters: { ids: string[] } }): Writable<Query<TransactionDTO[]>>
    queryTransactionsForMonthAndYear(period: { month: number, year: number }): Writable<Query<TransactionDTO[] | undefined>>
    queryTransactionById(period: { month: number, year: number }): Writable<Query<TransactionDTO[] | undefined>>
    queryTransactionsWithSearchTerm(
      existingStore: Writable<Query<TransactionDTO[]>>,
      searchTerm: string): Writable<Query<TransactionDTO[]>>

    addTransaction({ input: TransactionAddDTOInput }): Promise<Response<TransactionDTO>>
}

