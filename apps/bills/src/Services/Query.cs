using HotChocolate;
using Payobills.Bills.Services;

  public class Query
  {
    private readonly IBillsService? _billsService;
    public async Task<IEnumerable<Bill>> Bills([Service] IBillsService billsService) => await billsService.GetBillsAsync();
  }