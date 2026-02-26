import { LiteIndexedDbService } from "$lib/utils/lite/lite-indexed-db.service"
import { CONSTANTS } from "../../constants";
import type { BillDTO, LiteServices, TransactionDTO } from "$lib/types";
import { liteServices } from "$lib/stores/lite-services";

export const liteDb = new LiteIndexedDbService(CONSTANTS.DB_NAME)

export const setupLiteDemoAsync = async () => {
  return new Promise<void>((resolve) => {
  liteServices.subscribe(async (services: LiteServices) => {
    if(!services) resolve();

        const bills = [
            { id: "1", name: "Test Bill", payByDate: 15, billingDate: 2 },
            { id: "2", name: "Test Bill 2", payByDate: 11, billingDate: 1 },
            { id: "3", name: "Test Bill 3", payByDate: 13, billingDate: 12 }
        ] as BillDTO[];

        for(const bill of bills) {
          await services.billsService.addBill(bill)
        }

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

        for(const statement of billStatements) {
          await services.billStatementsService.addBillStatement(statement)
        }

        const transactions = [
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
        ]

        for(const transaction of transactions){
          await  services.transactionsService.addTransaction({ input: transaction })
        }

        resolve();

        })
    }
  )
}

