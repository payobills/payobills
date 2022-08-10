namespace payobills.bills.repos;

using payobills.bills.data;
using payobills.bills.dtos;
using payobills.bills.models;
using payobills.bills.svc;

public class BillRepo
{
    private readonly IGuidService guidService;
    private readonly IDateTimeService dateTimeService;
    private readonly BillsContext billsContext;

    public BillRepo(
        IGuidService guidService,
        IDateTimeService dateTimeService,
        BillsContext billsContext
    ) {
        this.guidService = guidService;
        this.dateTimeService = dateTimeService;
        this.billsContext = billsContext;
     }

    public async Task<Bill> AddBillAsync(BillDto dto)
    {
        var addResult = this.billsContext.Add(new Bill
        {
            Id = this.guidService.NewGuid(),
            Name = dto.Name,
            BillingDate = dto.BillingDate,
            PayByDate = dto.PayByDate,
            LatePayByDate = dto.LatePayByDate,
            CreatedAt = this.dateTimeService.UtcNow,
            UpdatedAt = this.dateTimeService.UtcNow
        });

        await this.billsContext.SaveChangesAsync();

        return addResult.Entity;
    }
}