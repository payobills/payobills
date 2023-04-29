using payobills.bills.models;
using payobills.bills.svc;

namespace payobills.bills.gql
{
  public class Query 
  {
    public async Task<IEnumerable<Bill>> Bills([Service] IBillsService billsService) => await billsService.GetBillsAsync();
  }
}
