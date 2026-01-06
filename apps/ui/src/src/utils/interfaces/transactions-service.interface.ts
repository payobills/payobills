import type { Writable } from "svelte/store";
import type {
	Query,
	Response,
	TransactionAddDTOInput,
	TransactionDTO,
} from "$lib/types";

export interface ITransactionsService {
	queryTransactions(input: {
		filters: { ids: string[] };
	}): Writable<Query<TransactionDTO[]>>;
	queryTransactionsForMonthAndYear(period: {
		month: number;
		year: number;
	}): Writable<Query<TransactionDTO[] | undefined>>;
	queryTransactionById(period: {
		month: number;
		year: number;
	}): Writable<Query<TransactionDTO[] | undefined>>;
	queryTransactionsWithSearchTerm(
		existingStore: Writable<Query<TransactionDTO[]>>,
		searchTerm: string,
	): Writable<Query<TransactionDTO[]>>;

	addTransaction({
		input: TransactionAddDTOInput,
	}): Promise<Response<TransactionDTO>>;
}
