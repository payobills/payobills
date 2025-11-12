import { Dexie, type EntityTable } from "dexie";

export class LiteIndexedDbService {
  private db: Dexie;

  public get bills() { return (this.db as any).bills } 
  public get billStatements() { return (this.db as any).billStatements } 
  public get transactions() { return (this.db as any).transactions } 
  constructor(dbName:string) {

        this.db = new Dexie(dbName) as Dexie & {
            bills: EntityTable<BillDTO, 'id'>;
            billStatements: EntityTable<BillStatementDTO, 'id'>
        };

        this.db.version(1).stores({
            bills: 'id, name, payByDate, billingDate',
            billStatements: 'id, billId, amount, cycleFromDate, cycleToDate, isFullyPaid',
            transactions: 'id, amount, merchant, notes, *receipts, paidAt'
        });

        (this.db as any).bills.bulkPut([
            { id: "1", name: "Test Bill", payByDate: 15, billingDate: 2 },
            { id: "2", name: "Test Bill 2", payByDate: 15, billingDate: 2 },
            { id: "3", name: "Test Bill 3", payByDate: 15, billingDate: 2 }
        ] as BillDTO[]);

        (this.db as any).billStatements.bulkPut([
            {
                id: "1",
                cycleFromDate: "2025-10-15",
                cycleToDate: "2025-11-15",
                amount: 1500,
                bill: { id: "1"}
            } as BillStatementDTO
        ]);
        (this.db as any).transactions.bulkPut([
            {
                id: "1",
                merchant: "Test Merchant",
                amount: 1500,
                paidAt: "2025-11-12T16:01:17.814Z" 
            } as TransactionDTO 
        ]);

    }

}
