import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { TypeOrmModule } from '@nestjs/typeorm'
import { Payment } from './entity/payment';
import { PaymentsController } from './payments/payments.controller';

@Module({
  imports: [
  TypeOrmModule.forRoot({
      type: 'sqlite',
      database: process.env.PAYMENTS_DB_PATH,
      entities: [Payment],
      synchronize: true,
    }),
  ],
  controllers: [AppController, PaymentsController],
  providers: [],
})
export class AppModule {}

