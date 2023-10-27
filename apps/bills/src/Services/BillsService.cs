using Payobills.Bills.Data.Contracts;
using Payobills.Bills.Services.Contracts;

namespace Payobills.Bills.Services;

public class BillsService : IBillsService
{
    private readonly IBillsRepo billRepo;

    public BillsService(BillsRepo billRepo) { this.billRepo = billRepo; }

    public Task<BillDTO> AddBillAsync(BillDTO dto)
    {
        var bill = billRepo.AddBillAsync(dto);
        return bill;
    }

    public async Task<IEnumerable<Bill>> GetBillsAsync()
    {
        var bills = billRepo.GetBillsAsync();
        return await Task.FromResult(bills);
    }
}