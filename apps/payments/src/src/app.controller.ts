import { Controller, Get } from '@nestjs/common';

@Controller()
export class AppController {
  @Get()
  getHello(): { app: string } {
    return { app: 'payments-service' }
  }
}
