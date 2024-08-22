import { Controller, Get } from "@nestjs/common";
import { UserPaymentDTO } from "./user-payment.dto";
import { Envelope } from "common/envelope";
import { ApiTags } from "@nestjs/swagger";

@ApiTags('payments')
@Controller({
    path: 'user-payments',
    version: 'v1',
})
export class UserPaymentsController {
    @Get()
    getUserPayments(): Envelope<UserPaymentDTO[]> {
        return {data: []}
    }
}