namespace payobills.bills.repos;

using payobills.bills.dtos;
using payobills.bills.models;
using payobills.bills.svc;

public class BillRepo
{
    private readonly IGuidService guidService;
    private readonly IDateTimeService dateTimeService;

    public BillRepo(
        IGuidService guidService,
        IDateTimeService dateTimeService
    ) {
        this.guidService = guidService;
        this.dateTimeService = dateTimeService;
     }

    public async Task<Bill> AddBillAsync(BillDto dto)
    {
        return await Task.FromResult(new Bill
        {
            Id = this.guidService.NewGuid(),
            Name = dto.Name,
            BillingDate = dto.BillingDate,
            PayByDate = dto.PayByDate,
            LatePayByDate = dto.LatePayByDate,
            CreatedAt = this.dateTimeService.UtcNow,
            UpdatedAt = this.dateTimeService.UtcNow
        });
    }
}