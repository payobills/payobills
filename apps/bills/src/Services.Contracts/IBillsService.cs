using Payobills.Bills.Services.Contracts.DTOs;

namespace Payobills.Bills.Services.Contracts;

public interface IBillsService
{
  Task<BillDTO> AddBillAsync(CreateBillDTO dto);
  Task<IEnumerable<BillDTO>> GetBillsAsync();
  Task<BillDTO?> GetBillByIdAsync(Guid id);
}