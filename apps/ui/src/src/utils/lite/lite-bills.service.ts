import { writable, type Writable } from "svelte/store";
import type { IBillsService } from "../interfaces/bills-service.interface";
import { type BillDTO, type AddBillDTO, type BillStatementDTO, type Query, type TransactionDTO } from "$lib/types";
import type { LiteIndexedDbService } from "./lite-indexed-db.service";

export class LiteBillService implements IBillsService {
    constructor(private dbService: LiteIndexedDbService) { }

    private bills = writable<Query<BillDTO[] | undefined>>({
        fetching: false,
        data: undefined,
        error: null
    });

    queryBillById(id: string): Writable<Query<BillDTO | undefined>> {
        const billByIdQuery = writable<Query<BillDTO | undefined>>({
          fetching: true,
          data: undefined,
          error: null
        }); 

        (async () => {
            try {
                const matchingBills = await this.dbService.bills
                  .where({ id })
                  .toArray();

                billByIdQuery.update(prevState => ({
                    ...prevState,
                    fetching: false,
                    data: matchingBills.length === 1 ? matchingBills[0] : undefined
                }));
            }
            catch (err) { console.error(err); }

        })();

        return billByIdQuery;
    }

    queryBills(): Writable<Query<BillDTO[] | undefined>> {
        (async () => {
            try {
                const bills = await this.dbService.bills.toArray();
                this.bills.update(prevState => ({
                    ...prevState,
                    data: bills
                }));
            }
            catch (err) { console.error(err); }

        })();

        return this.bills;
    }

    updateBill(id: string, bill: AddBillDTO): Promise<unknown> {
        // TODO: Fix bill type
        const currentTimeStamp = new Date()
        const billToUpdate: BillDTO = {
            ...bill,
            updatedAt: currentTimeStamp
        }

        console.log(billToUpdate)

        return new Promise(async (resolve, reject) => {
            try {
            await this.dbService.bills.update(id, billToUpdate);
            this.bills.update(prevState => ({
                ...prevState,
                data: [...prevState?.data || [], billToUpdate]
            }));
            resolve(bill);
      }catch(err){console.error(err);reject()}
        });
    }
}

