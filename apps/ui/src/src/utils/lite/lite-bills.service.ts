import { writable, type Writable } from "svelte/store";
import type { IBillsService } from "../interfaces/bills-service.interface";
import type { BillDTO, BillStatementDTO, Query, TransactionDTO } from "$lib/types";
import type { LiteIndexedDbService } from "./lite-indexed-db.service";

export class LiteBillService implements IBillsService {
    constructor(private dbService: LiteIndexedDbService) { }

    private bills = writable<Query<BillDTO[] | undefined>>({
        fetching: false,
        data: undefined,
        error: null
    });

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
}

