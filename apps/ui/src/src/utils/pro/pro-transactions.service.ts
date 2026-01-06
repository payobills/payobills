import { type Client, gql } from "@urql/svelte";
import { type Writable, writable } from "svelte/store";
import type { Query, TransactionDTO } from "$lib/types";
import type { ITransactionsService } from "../interfaces/transactions-service.interface";

export class ProTransactionsService implements ITransactionsService {
	constructor(private transactionUrqlClient: Client) {}
	private transactionsForCurrentMonth = writable<
		Query<TransactionDTO[] | undefined>
	>({
		fetching: false,
		data: undefined,
		error: null,
	});

	queryTransactionsWithSearchTerm(
		existingStore: Writable<Query<TransactionDTO[]>>,
		searchTerm: string,
	): Writable<Query<TransactionDTO[]>> {
		const matchingTransactionsQuery: Writable<Query<TransactionDTO[]>> =
			existingStore ??
			writable({
				fetching: true,
				data: [],
				error: null,
			});

		(async () => {
			try {
				const matchingTransactions = await this.transactionUrqlClient
					.query(
						`{
        transactions(filters: {searchTerm: "${searchTerm}"}) {
        nodes {
        id
        amount
        merchant
        paidAt
        transactionText
        notes
        }
        }
      }`,
						{},
					)
					.toPromise();

				// console.log(matchingTransactions?.data?.transactions.nodes )
				matchingTransactionsQuery.update((curr) => ({
					...curr,
					data: matchingTransactions?.data?.transactions.nodes ?? [],
					fetching: false,
					error: null,
				}));
			} catch (err) {
				matchingTransactionsQuery.update((curr) => ({
					...curr,
					data: [],
					fetching: false,
					error: `Couldn't find any matching transactions for this search.`,
				}));
			}
		})();

		return matchingTransactionsQuery;
	}

	queryTransactionsForCurrentMonth(): Writable<
		Query<TransactionDTO[] | undefined>
	> {
		throw new Error("Not implemented");
	}
}
