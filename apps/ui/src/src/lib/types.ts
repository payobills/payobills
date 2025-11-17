// HELPERS

export type ID<T> = {
    id: T
}

// DTOs

export type AddBillDTO = {
    name: string
    billingDate?: number
    payByDate?: number
}

export type AddBillStatementDTO = {
    notes: string
    amount: number
    bill: ID<string>
    startDate: string,
    endDate: string,
    isFullyPaid: boolean
    edges: { paymentIds: string[] }
}

// LITE INDEXED DB TYPES

export type AddLiteBillStatementDTO = {
    notes: string
    amount: number
    billId: string
    cycleFromDate: string,
    cycleToDate: string,
    isFullyPaid: boolean
    edges: { paymentIds: string[] }
}

export type LiteBillStatementDTO = { id: string } & AddLiteBillStatementDTO;

// MODELS

export type Query<T> = {
    fetching: boolean;
    data: T;
    error: unknown | null | undefined;
}

export type BillDTO = {
    id: string
    isEnabled: boolean
    createdAt: Date
    updatedAt: Date
    payments: any[]
} & AddBillDTO


export type BillStatDTO = {
    startDate: Date,
    endDate: Date,
    stats: {
        type: string
        billIds: string[]
        dateRanges: {
            start: number
            end: number
        }[]
    }
}

export type TransactionDTO = {
    id: string
    amount: number
    merchant: string | null
    notes: string
    receipts: any[]
    updatedReceipts: any[]
    paidAt: string
}

export type BillStatementDTO = {
    id: string
} & AddBillStatementDTO