import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { UserPaymentsController } from 'user-payments/user-payments.controller';

@Module({
  imports: [],
  controllers: [
    AppController,
    UserPaymentsController
  ],
  providers: [],
})
export class AppModule {}
