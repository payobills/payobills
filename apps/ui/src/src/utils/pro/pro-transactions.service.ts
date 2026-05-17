import { writable, type Writable } from "svelte/store";
import type { ITransactionsService } from "../interfaces/transactions-service.interface";
import type { TransactionDTO, Query } from "$lib/types";
import { gql, type Client } from "@urql/svelte";

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
		// console.log({existingStore, searchTerm})
		// const matchingTransactionsQuery: Writable<Query<TransactionDTO[]>> = !existingStore ? writable({
		//   fetching: true,
		//   data: [],
		//   error: null
		// }) : existingStore;

		// console.log('q', matchingTransactionsQuery );
		(async () => {
			try {
				// console.log('checking matches')
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

				// console.log('got some', matchingTransactions);
				existingStore?.update((curr) => ({
					...curr,
					data: matchingTransactions?.data?.transactions.nodes ?? [],
					fetching: false,
					error: null,
				}));
			} catch (err) {
				// console.log(err)
				existingStore?.update((curr) => ({
					...curr,
					data: [],
					fetching: false,
					error: `Couldn't find any matching transactions for this search.`,
				}));
			}
		})();
	}

	async loadMoreTransactions(
		existingStore: Writable<any>,
		after: string | null,
		first: number = 10,
	): Promise<void> {
		const result = await this.transactionUrqlClient
			.query(
				`query ($first: Int!, $after: String) {
          transactions(first: $first, after: $after, order: [{ paidAt: DESC }]) {
            pageInfo { endCursor hasNextPage }
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
				{ first, after },
			)
			.toPromise();
		existingStore.update((curr: any) => {
			const existingIds = new Set((curr.data ?? []).map((t: any) => t.id));
			const newNodes = (result?.data?.transactions?.nodes ?? []).filter(
				(t: any) => !existingIds.has(t.id),
			);
			return {
				...curr,
				data: [...(curr.data ?? []), ...newNodes],
				endCursor: result?.data?.transactions?.pageInfo?.endCursor ?? null,
				hasNextPage: result?.data?.transactions?.pageInfo?.hasNextPage ?? false,
			};
		});
	}

	queryTransactionsForCurrentMonth(): Writable<
		Query<TransactionDTO[] | undefined>
	> {
		throw new Error("Not implemented");
	}
}
