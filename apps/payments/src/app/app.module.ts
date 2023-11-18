import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { UserPaymentsController } from 'user-payments/user-payments.controller';
import { AppService } from './app.service';

@Module({
  imports: [],
  controllers: [
    AppController,
    UserPaymentsController
  ],
  providers: [AppService],
})
export class AppModule {}
