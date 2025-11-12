import type { AddBillStatementDTO, BillStatementDTO, BillDTO, Query } from "$lib/types";
import { writable, type Writable } from "svelte/store";
import type { IBillStatementsService } from "../interfaces/bill-statements-service.interface";

export class LiteBillStatementsService implements IBillStatementsService {
    constructor(private dbService: LiteIndexedDbService) { }

    private billStatements = writable<Query<BillStatementDTO[] | undefined>>({
        fetching: false,
        data: undefined,
        error: null
    });

    queryBillStatementsByBillIds(_: string[]): Writable<Query<BillStatementDTO[] | undefined>> {
       (async () => {
            try {
                const billStatements = await this.dbService.billStatements.toArray();
                this.billStatements.update(prevState => ({
                    ...prevState,
                    data: billStatements
                }));
            }
            catch (err) { console.error(err); }

        })();
    }
    
    addBillStatement(_: AddBillStatementDTO): Promise<unknown> {
        return Promise.resolve();
    }
}
