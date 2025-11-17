import type { AddBillStatementDTO, BillStatementDTO, BillDTO, Query, LiteBillStatementDTO } from "$lib/types";
import { writable, type Writable } from "svelte/store";
import type { IBillStatementsService } from "../interfaces/bill-statements-service.interface";
import type { LiteIndexedDbService } from "./lite-indexed-db.service";
import { CONSTANTS } from "../../constants";

export class LiteBillStatementsService implements IBillStatementsService {
    constructor(private dbService: LiteIndexedDbService) { }

    private billStatements = writable<Query<{ [key: string]: BillStatementDTO[] } | undefined>>({
        fetching: false,
        data: undefined,
        error: null
    });

    queryBillStatementsByBillIds(billIds: string[]): Writable<Query<{ [key: string]: BillStatementDTO[] } | undefined>> {
        (async () => {
            try {
                const billStatements: LiteBillStatementDTO[] = await this.dbService.billStatements.toArray();
                const billStatementsGroupedByBills = billStatements.reduce((
                    acc: { [key: string]: BillStatementDTO[] },
                    curr: LiteBillStatementDTO) => {

                    const currAsBillStatementDTO: BillStatementDTO = {
                        ...curr,
                        bill: { id: curr.billId },
                        startDate: curr.cycleFromDate,
                        endDate: curr.cycleToDate
                    }

                    const currBillStatementKey = `${CONSTANTS.BILL_STATEMENTS_KEY_PREFIX}${curr.billId}`
                    if (!acc[currBillStatementKey]) {
                        acc[currBillStatementKey] = [currAsBillStatementDTO];
                    } else {
                        acc[currBillStatementKey].push(currAsBillStatementDTO);
                    }

                    return acc;
                }, {})

                billIds.forEach(billId => billStatementsGroupedByBills[`${CONSTANTS.BILL_STATEMENTS_KEY_PREFIX}${billId}`] ||= []); 

                this.billStatements.update(prevState => ({
                    ...prevState,
                    data: billStatementsGroupedByBills,
                    error: null,
                    fetching: false
                }));
            }
            catch (err) { console.error(err); }

        })();

        return this.billStatements;
    }

    async addBillStatement(dto: AddBillStatementDTO): Promise<unknown> {
        // TODO: Add Validation to check if billStatement already exists and data is valid

        const l = {
            id: crypto.randomUUID(),
            ...dto,
            cycleFromDate: dto.startDate,
            cycleToDate: dto.endDate,
            bill: undefined,
            billId: dto.bill.id
        }

        console.log('adding ', l)
        await this.dbService.billStatements.add(l);

        return dto;
    }
}
