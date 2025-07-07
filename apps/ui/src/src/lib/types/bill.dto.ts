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
