using System.Text.Json.Nodes;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;

// [ExtendObjectType("Query")]
public class Query
{
  public async Task<IEnumerable<BillDTO>> Bills([Service] IBillsService billsService)
    => await billsService.GetBillsAsync();

    public async Task<BillDTO?> BillById([Service] IBillsService billsService, string id)
    => await billsService.GetBillByIdAsync(id);

    public async Task<BillStatsDTO> BillStats([Service] StatsQueryService statsQueryService, int year, int month)
    => await statsQueryService.BillStats(year, month);

    public async Task<IEnumerable<BillStatementDTO>> BillStatements([Service] IBillStatementsService billStatementsService, string billId)
    => await billStatementsService.GetBillStatementsAsync(billId);

    public async Task<BillStatementDTO?> BillStatementById([Service] IBillStatementsService billStatementsService, string billStatementId)
    => await billStatementsService.GetBillStatementByIdAsync(billStatementId);
}
