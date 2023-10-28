using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;
public class Mutation
{
  public async Task<BillDTO> AddBill([Service] IBillsService billsService, CreateBillDTO billDto)
    => await billsService.AddBillAsync(billDto);
}
