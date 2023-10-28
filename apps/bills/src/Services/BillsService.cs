using Payobills.Bills.Data.Contracts;
using Payobills.Bills.Data.Contracts.Models;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;

namespace Payobills.Bills.Services;

public class BillsService : IBillsService
{
    private readonly IBillsRepo billRepo;

    public BillsService(IBillsRepo billRepo) { this.billRepo = billRepo; }

    public async Task<BillDTO> AddBillAsync(BillDTO dto)
    {
        Bill billToAdd = new Bill
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            BillingDate = dto.BillingDate,
            CreatedAt =DateTime.UtcNow,
            UpdatedAt  =DateTime.UtcNow,
            LatePayByDate = dto.LatePayByDate,
            PayByDate = dto.PayByDate
        };

        var bill = await billRepo.AddBillAsync(billToAdd);
        return new BillDTO {

        };
    }

    public Task<IEnumerable<BillDTO>> GetBillsAsync()
    {
        // var bills = billRepo.GetBillsAsync();
        return Task.FromResult(Array.Empty<BillDTO>().AsEnumerable());
    }
}