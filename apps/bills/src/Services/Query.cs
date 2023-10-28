using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;

public class Query
{
  public async Task<IEnumerable<BillDTO>> Bills([Service] IBillsService billsService)
    => await billsService.GetBillsAsync();
}