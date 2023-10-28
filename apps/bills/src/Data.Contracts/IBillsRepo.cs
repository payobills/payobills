using Payobills.Bills.Data.Contracts.Models;

namespace Payobills.Bills.Data.Contracts;

public interface IBillsRepo
{
     Task<Bill> AddBillAsync(Bill dto);
     IQueryable<Bill> GetBillsAsync();
}
