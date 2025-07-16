using Payobills.Bills.Services.Contracts.DTOs;

namespace Payobills.Bills.Services.Contracts;

public interface IBillStatementsService
{
  Task<IEnumerable<BillStatementDTO>> GetBillStatementsAsync(string billId);
  Task<BillStatementDTO?> GetBillStatementByIdAsync(string billStatementId);
  Task<BillStatementDTO> AddOrUpdateBillStatementAsync(AddOrUpdateBillStatementDTO input);
}
