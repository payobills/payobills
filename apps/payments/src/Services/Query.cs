using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services;

public class Query
{
  [UsePaging]
  [UseSorting]
  [NodeResolver]
  public async Task<IEnumerable<TransactionDTO>> Transactions([Service] ITransactionsService transactionsService)
    => await transactionsService.GetTransactionsAsync(null!);

  public async Task<TransactionDTO> TransactionByID([Service] ITransactionsService transactionsService, string id)
    => await transactionsService.GetTransactionByIDAsync(id);

  [UsePaging(DefaultPageSize = 1000, MaxPageSize = 1000)]
  [UseSorting]
  public async Task<IEnumerable<TransactionDTO>> TransactionsByYearAndMonth([Service] ITransactionsService transactionsService, int year, int month)
    => await transactionsService.GetTransactionsByYearAndMonthAsync(year, month);

  public async Task<IEnumerable<TransactionTagDTO>> TransactionTags([Service] ITransactionsService transactionsService)
    => await transactionsService.GetTransactionTagsAsync();
}
