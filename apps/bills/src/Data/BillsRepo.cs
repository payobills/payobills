namespace payobills.bills.repos;

using System;
using payobills.bills.data;
using payobills.bills.dtos;
using payobills.bills.models;
using Payobills.Bills.Services;

public class BillsRepo
{
    private readonly IGuidService guidService;
    private readonly IDateTimeService dateTimeService;
    private readonly BillsContext billsContext;

    public BillsRepo(
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

    public IQueryable<Bill> GetBillsAsync()
    {
        return this.billsContext.Bills.AsQueryable();
    }
}