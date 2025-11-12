import { writable, type Writable } from "svelte/store";
import type { ITransactionsService } from "../interfaces/transactions-service.interface";
import type { TransactionDTO, Query } from "$lib/types";

export class LiteTransactionsService implements ITransactionsService {
    constructor(private dbService: LiteIndexedDbService) { }
    private transactionsForCurrentMonth = writable<Query<TransactionDTO[] | undefined>>({
        fetching: false,
        data: undefined,
        error: null
    });

    queryTransactionsForCurrentMonth(): Writable<Query<TransactionDTO[] | undefined>> {
      (async () => {
            try {
                const transactions = await this.dbService.transactions.toArray();
                this.transactionsForCurrentMonth.update(prevState => ({
                    ...prevState,
                    data: transactions 
                }));
            }
            catch (err) { console.error(err); }

        })();
  
        return this.transactionsForCurrentMonth
    }
    
}
