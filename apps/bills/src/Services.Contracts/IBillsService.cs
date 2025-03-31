using Payobills.Bills.Services.Contracts.DTOs;

namespace Payobills.Bills.Services.Contracts;

public interface IBillsService
{
  Task<BillDTO> AddBillAsync(CreateBillDTO dto);
  Task<IEnumerable<BillDTO>> GetBillsAsync();
  Task<IEnumerable<BillDTO>> GetBillsByIdsAsync(IEnumerable<string> ids);
  Task<BillDTO?> GetBillByIdAsync(string id);
  Task<PaymentDTO> MarkPaymentForBillAsync(MarkPaymentForBillDTO dto);
}