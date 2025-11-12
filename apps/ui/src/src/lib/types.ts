export type Query<T> = {
    fetching: boolean;
    data: T;
    error: unknown | null | undefined;
}

export type BillDTO = {
    id: string
    name: string
    billingDate?: number
    payByDate?: number
    latePayByDate?: number
    payments: any[];
    isEnabled: boolean
    createdAt: Date;
    updatedAt?: Date;
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

export type ID<T> = {
    id: T
}

export type AddBillStatementDTO = {
    amount: number
    bill: ID<string>,
    cycleFromDate: string
    cycleToDate: string,
    isFullyPaid: boolean
}

export type BillStatementDTO = {
    id: string
} & AddBillStatementDTO
