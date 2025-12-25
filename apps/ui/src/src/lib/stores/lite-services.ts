import { readable, writable } from "svelte/store";
import { LiteIndexedDbService } from "../../utils/lite/lite-indexed-db.service"
import { LiteTransactionsService } from "../../utils/lite/lite-transactions.service";
import { LiteBillStatementsService } from "../../utils/lite/lite-bill-statements.service";
import { LiteBillService } from "../../utils/lite/lite-bills.service";
import type { IBillsService } from "../../utils/interfaces/bills-service.interface";
import type { IBillStatementsService } from "../../utils/interfaces/bill-statements-service.interface";
import type { ITransactionsService } from "../../utils/interfaces/transactions-service.interface";
import { CONSTANTS } from "../../constants";
import { subscribe } from "graphql";

export const liteDb = new LiteIndexedDbService(CONSTANTS.DB_NAME)

export type LiteServices = {
  billsService: IBillsService
  billStatementsService: IBillStatementsService
  transactionsService: ITransactionsService
}

const __liteServices = writable<LiteServices | null>(null);

export const setupLiteServices = () => {
  __liteServices.subscribe(p => {
    if(p) return;
  
    __liteServices.set({
      billsService: new LiteBillService(liteDb),
      billStatementsService: new LiteBillStatementsService(liteDb),
      transactionsService: new LiteTransactionsService(liteDb),
    });
  });
}

export const liteServices = {
  subscribe: __liteServices.subscribe
  // get: __liteServices.get
} 

