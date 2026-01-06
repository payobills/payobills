import { type Writable, writable } from "svelte/store";
import type {
	AddBillStatementDTO,
	BillDTO,
	BillStatDTO,
	BillStatementDTO,
	Query,
} from "$lib/types";
import type { IBillStatementsService } from "../interfaces/bill-statements-service.interface";
import type { IBillStatsService } from "../interfaces/bill-stats-service.interface";
import type { LiteIndexedDbService } from "./lite-indexed-db.service";

export class LiteBillStatsService implements IBillStatsService {
	constructor(private dbService: LiteIndexedDbService) {}

	private billStats = writable<Query<BillStatDTO | undefined>>({
		fetching: false,
		data: undefined,
		error: null,
	});

	queryBillStats(
		year: string,
		month: string,
	): Writable<Query<BillStatDTO | undefined>> {
		(async () => {
			try {
				// const billStatements =
				// const billStats = []
				// await this.dbService.billStatements.toArray();
				// this.billStats.update(prevState => ({
				//     ...prevState,
				//     data: billStatements
				// }));
			} catch (err) {
				console.error(err);
			}
		})();

		return this.billStats;
	}

	async addBillStatement(dto: AddBillStatementDTO): Promise<unknown> {
		// TODO: Add Validation to check if billStatement already exists and data is valid

		await this.dbService.billStatements.add(dto);
		return dto;
	}
}
