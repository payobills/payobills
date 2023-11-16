using Payobills.Bills.Data.Contracts.Models;

namespace Payobills.Bills.Data.Contracts;

public interface IBillsRepo
{
     Task<Bill> AddBillAsync(Bill dto);
     Task<Bill> UpdateBillAsync(Bill dto);
     IQueryable<Bill> GetBillsAsync();
     IQueryable<Bill> GetBillByIdAsync(Guid id);
}
