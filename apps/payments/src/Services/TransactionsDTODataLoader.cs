using Payobills.Payments.Services.Contracts.DTOs;
using Payobills.Payments.Services.Contracts;

namespace Payobills.Bills.Services;

public class TransactionDTODataLoader(
    ITransactionsService billsService,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options = null) : BatchDataLoader<string, TransactionDTO>(batchScheduler, options)
{
    private readonly ITransactionsService billsService = billsService;

    protected override async Task<IReadOnlyDictionary<string, TransactionDTO>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken)
    {
        var transactions =  await billsService.GetTransactionsByIDsAsync(keys.AsEnumerable());
        return transactions.ToDictionary(x => x.Id.ToString());
    }
}