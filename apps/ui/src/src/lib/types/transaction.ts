export type Transaction = {
    amount: number
    merchant: string | null
    notes: string
    receipts: any[]
    updatedReceipts: any[]
}
