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
                    data: matchingBills.length == 1 ? matchingBills[0] : undefined
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

    addBill(bill: AddBillDTO): Promise<unknown> {
        // TODO: Fix bil; type
        const currentTimeStamp = new Date()
        const billToAdd: BillDTO = {
            ...bill, id: crypto.randomUUID(),
            payments: [],
            isEnabled: true,
            createdAt: currentTimeStamp,
            updatedAt: currentTimeStamp
        }

        return new Promise(async resolve => {
            await this.dbService.bills.add(billToAdd);
            this.bills.update(prevState => ({
                ...prevState,
                data: [...prevState?.data || [], billToAdd]
            }));
            resolve(bill);
        });
    }
}

