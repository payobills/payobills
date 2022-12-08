using payobills.bills.models;
using payobills.bills.svc;
using payobills.bills.dtos;

namespace payobills.bills.gql;

public class Mutation
{
  public async Task<Bill> AddBill([Service] IBillsService billsService, BillDto billDto)
  {
    var addedBill = await billsService.AddBillAsync(billDto);
    return addedBill;
  }
}
