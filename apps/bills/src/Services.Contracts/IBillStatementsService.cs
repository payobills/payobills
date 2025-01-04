using Payobills.Bills.Services.Contracts.DTOs;

namespace Payobills.Bills.Services.Contracts;

public interface IBillStatementsService
{
  Task<IEnumerable<BillStatementDTO>> GetBillStatementsAsync();
}