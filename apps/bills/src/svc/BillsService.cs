using payobills.bills.dtos;
using payobills.bills.models;
using payobills.bills.repos;

namespace payobills.bills.svc;

public class BillsService : IBillsService
{
    private readonly BillsRepo billRepo;

    public BillsService(BillsRepo billRepo) { this.billRepo = billRepo; }

    public Task<Bill> AddBillAsync(BillDto dto)
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