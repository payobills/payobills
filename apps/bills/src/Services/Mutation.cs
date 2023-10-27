using HotChocolate;
public class Mutation
{
  public async Task<Bill> AddBill([Service] IBillsService billsService, BillDTO billDto)
  {
    var addedBill = await billsService.AddBillAsync(billDto);
    return addedBill;
  }
}
