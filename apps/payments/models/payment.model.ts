export class Payment {
  id: string
  billStartDate: Date;
  BillEndDate: Date
  BillPayByDate: Date
  CreatedAt: Date
  // get isLate(): bool { return createdAt > billPayByDate };
}
