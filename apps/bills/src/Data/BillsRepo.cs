using MongoDB.Driver;
using Payobills.Bills.Data.Contracts;
using Payobills.Bills.Data.Contracts.Models;
using System.Linq;

namespace Payobills.Bills.Data;

public class BillsRepo : IBillsRepo
{
    private readonly IBillsContext billsContext;

    public BillsRepo(IBillsContext billsContext)
    {
        this.billsContext = billsContext;
    }

    public async Task<Bill> AddBillAsync(Bill input)
    {
        await this.billsContext.Bills.InsertOneAsync(input);
        return input;
    }

    public IQueryable<Bill> GetBillsAsync() => this.billsContext.Bills.AsQueryable();
    public IQueryable<Bill> GetBillByIdAsync(Guid id) => this.billsContext.Bills
        .AsQueryable()
        .Where(p => p.Id == id);
}