namespace Payobills.Bills.Services.Contracts;

public interface IBillsService
{
  Task<BillDTO> AddBillAsync(BillDTO dto);
  Task<IEnumerable<BillDTO>> GetBillsAsync();
}