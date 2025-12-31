import { writable, type Writable } from "svelte/store";
import type { ITransactionsService } from "../interfaces/transactions-service.interface";
import type { TransactionDTO, Query, TransactionAddDTOInput } from "$lib/types";
import type { LiteIndexedDbService } from "./lite-indexed-db.service";

export class LiteTransactionsService implements ITransactionsService {
    constructor(private dbService: LiteIndexedDbService) { }
    private transactionsForCurrentMonth = writable<Query<TransactionDTO[] | undefined>>({
        fetching: false,
        data: undefined,
        error: null
    });

    async addTransaction({ input }: { input: TransactionAddDTOInput }): Promise<TransactionDTO<TransactionDTO>> {
        const addedTransaction = await this.dbService.transactions.add({
          ...input,
          id: crypto.randomUUID(),
          paidAt: new Date()
        });

       const addedDto: TransactionDTO = {
          ...addedTransaction        
        } 

        return addedDto 
    }

    async updateTransaction(id: string, dto: TransactionAddDTOInput): Promise<TransactionDTO> {
        const transaction = await this.dbService.transactions.update(id, {
          ...dto,
          id,
          updatedAt: new Date()
        });

       // const updatedDto: TransactionDTO = {
          // ...transaction        
       // }; 

      return transaction
    }

    queryTransactionsWithSearchTerm(
      existingStore: Writable<Query<TransactionDTO[]>>,
      searchTerm: string): Writable<Query<TransactionDTO[]>> {
      throw new Error('Not Implemented')
    }

    queryTransactions(input: { filters: { ids: string[] } }): Writable<Query<TransactionDTO[]>> {
      const transactionsStore = writable<Query<TransactionDTO[]>>({
        fetching: false,
        data: [],
        error: null
      });

      (async () => {
            try {
                // const transactionsLowerBound = new Date(year, month === 1 ? 11 : month - 1, 1) 
                // const transactionsUpperBound = new Date(month < 12 ? year : year + 1, month < 12 ? month : month, 1);
                // console.table({transactionsUpperBound, transactionsLowerBound})
                const ids = input?.filters?.ids
        
                if (ids.length === 0) throw new Error('No filters added')
                
                const transactions = await this.dbService.transactions
                  .where('id')
                  .anyOf(ids)
                  .toArray();

                transactionsStore.update(prevState => ({
                    ...prevState,
                    data: transactions 
                }));
            }
            catch (err) {
              console.error(err);

              transactionsStore.update(prev => ({
                ...prev,
                fetching: false,
                data: [],
                error: err
              }));
            }

        })();
  
        return transactionsStore
    }

    queryTransactionsForMonthAndYear(period: { month: number, year: number }): Writable<Query<TransactionDTO[] | undefined>> {
      (async () => {
            try {
                const month = period.month; const year = period.year; 
                // console.table({ month, year })
        
                const transactionsLowerBound = new Date(year, month === 1 ? 11 : month - 1, 1) 
                const transactionsUpperBound = new Date(month < 12 ? year : year + 1, month < 12 ? month : month, 1);
                // console.table({transactionsUpperBound, transactionsLowerBound})

                const transactions = await this.dbService.transactions
                  .where('paidAt')
                  .between(transactionsLowerBound, transactionsUpperBound, true, false)
                  .toArray();

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

