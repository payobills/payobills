import { Column, Entity, PrimaryColumn } from 'typeorm'

@Entity()
export class Payment {
  @PrimaryColumn()
  id: string

  @Column()
  billStartDate: Date

  @Column()
  BillEndDate: Date

  @Column()
  BillPayByDate: Date

  @Column()
  CreatedAt: Date

  // get isLate(): bool { return createdAt > billPayByDate };
}

