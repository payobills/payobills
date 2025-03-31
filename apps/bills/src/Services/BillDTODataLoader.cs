using Payobills.Bills.Services.Contracts.DTOs;
using Payobills.Bills.Services.Contracts;

namespace Payobills.Bills.Services;

public class BillDTODataLoader : BatchDataLoader<string, BillDTO>
{
    private readonly IBillsService billsService;

    public BillDTODataLoader(
        IBillsService billsService,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options = null)
        : base(batchScheduler, options)
    {
        this.billsService = billsService;
    }

    protected override async Task<IReadOnlyDictionary<string, BillDTO>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken)
    {
        var bills =  await billsService.GetBillsByIdsAsync(keys.AsEnumerable());
        return bills.ToDictionary(x => x.Id);
    }
}
