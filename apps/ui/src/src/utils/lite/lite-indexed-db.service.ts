import { Dexie, type EntityTable, type Table } from "dexie";
import type { BillDTO, BillStatementDTO } from "$lib/types";

export class LiteIndexedDbService {
	private db: Dexie;

	public get bills() {
		return (this.db as any).bills as Table;
	}
	public get billStatements() {
		return (this.db as any).billStatements as Table;
	}
	public get transactions() {
		return (this.db as any).transactions as Table;
	}
	constructor(dbName: string) {
		this.db = new Dexie(dbName) as Dexie & {
			bills: EntityTable<BillDTO, "id">;
			billStatements: EntityTable<BillStatementDTO, "id">;
		};

		this.db.version(1).stores({
			bills: "id, name, payByDate, billingDate",
			billStatements:
				"id, notes, billId, amount, cycleFromDate, cycleToDate, isFullyPaid, edges",
			transactions: "id, amount, merchant, notes, *receipts, paidAt",
		});
	}
}
