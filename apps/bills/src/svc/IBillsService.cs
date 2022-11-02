using payobills.bills.dtos;
using payobills.bills.models;

namespace payobills.bills.svc;

public interface IBillsService
{
  Task<Bill> AddBillAsync(BillDto dto);
  Task<IEnumerable<Bill>> GetBillsAsync();
}