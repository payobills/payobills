import type { LiteBillStatementDTO, BillDTO, BillStatementDTO, TransactionDTO } from "$lib/types";
import { Dexie, type EntityTable, type Table } from "dexie";

export class LiteIndexedDbService {
    private db: Dexie;

    public get bills() { return (this.db as any).bills as Table }
    public get billStatements() { return (this.db as any).billStatements as Table }
    public get transactions() { return (this.db as any).transactions as Table }
    constructor(dbName: string) {

        this.db = new Dexie(dbName) as Dexie & {
            bills: EntityTable<BillDTO, 'id'>;
            billStatements: EntityTable<BillStatementDTO, 'id'>
        };

        this.db.version(1).stores({
            bills: 'id, name, payByDate, billingDate',
            billStatements: 'id, notes, billId, amount, cycleFromDate, cycleToDate, isFullyPaid, edges',
            transactions: 'id, amount, merchant, notes, *receipts, paidAt'
        });

        (this.db as any).bills.bulkPut([
            { id: "1", name: "Test Bill", payByDate: 15, billingDate: 2 },
            { id: "2", name: "Test Bill 2", payByDate: 11, billingDate: 1 },
            { id: "3", name: "Test Bill 3", payByDate: 13, billingDate: 12 }
        ] as BillDTO[]);

        const billStatements = [{
            id: "1",
            notes: "",
            edges: { paymentIds: ["1"] },
            cycleFromDate: "2025-10-15",
            cycleToDate: "2025-11-15",
            amount: 1500,
            billId: "1",
            isFullyPaid: true
        }];

        (this.db as any).billStatements.bulkPut(billStatements);
        (this.db as any).transactions.bulkPut([
            {
                id: "1",
                merchant: "Test Merchant",
                amount: 1500,
                paidAt: new Date("2025-11-12T16:01:17.814Z"),
                parseStatus: "Parsed",
                bill: { id: 1 }
            } as TransactionDTO,
            {
                id: "2",
                merchant: "Test Merchant 2",
                amount: 499,
                paidAt: new Date("2025-12-12T16:01:17.814Z"),
                parseStatus: "Parsed",
                bill: { id: 2 }
            } as TransactionDTO

        ]);

    }

}
