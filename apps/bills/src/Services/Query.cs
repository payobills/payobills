using System.Text.Json.Nodes;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;

public class Query
{
  public async Task<IEnumerable<BillDTO>> Bills([Service] IBillsService billsService)
    => await billsService.GetBillsAsync();

    public async Task<BillDTO?> BillById([Service] IBillsService billsService, string id)
    => await billsService.GetBillByIdAsync(id);

    public async Task<BillStatsDTO> BillStats([Service] StatsQueryService statsQueryService, int year, int month)
    => await statsQueryService.BillStats(year, month);
}
