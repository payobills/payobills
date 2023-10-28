import { Controller, Get } from '@nestjs/common';

@Controller({
  path: '',
  version: 'v1'
})
export class AppController {
  @Get()
  getHello(): { app: string } {
    return { app: 'payments-service' }
  }
}
