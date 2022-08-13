using payobills.bills.dtos;
using payobills.bills.models;

namespace payobills.bills.svc;

public interface IBillService
{
  Task<Bill> AddBillAsync(BillDto dto);
}