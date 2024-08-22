export class UserPaymentDTO
{
    public id: string;
    public billId: string;
    public amount: number;
    public amountCurrency: "INR" | "USD";
    public createdAt: Date;
}