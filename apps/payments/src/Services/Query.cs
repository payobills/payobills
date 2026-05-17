using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services;

public class Query
{
  [UsePaging]
  [UseSorting]
  [NodeResolver]
  public async Task<IEnumerable<TransactionDTO>> Transactions([Service] ITransactionsService transactionsService,
  TransactionFiltersInput? filters = null)
    => await transactionsService.GetTransactionsAsync(null!, filters);

  public async Task<TransactionDTO> TransactionByID([Service] ITransactionsService transactionsService, string id)
    => await transactionsService.GetTransactionByIDAsync(id);

  [UsePaging(DefaultPageSize = 1000, MaxPageSize = 1000)]
  [UseSorting]
  public async Task<IEnumerable<TransactionDTO>> TransactionsByYearAndMonth([Service] ITransactionsService transactionsService, int year, int month)
    => await transactionsService.GetTransactionsByYearAndMonthAsync(year, month);

  public async Task<IEnumerable<TransactionTagDTO>> TransactionTags([Service] ITransactionsService transactionsService)
    => await transactionsService.GetTransactionTagsAsync();

<<<<<<< HEAD
  public async Task<IEnumerable<TransactionStatDTO>> TransactionStats(
    [Service] ITransactionStatsService statsService,
    TransactionStatsFiltersInput? filters = null)
    => await statsService.GetTransactionStatsAsync(filters);
=======
  public async Task<IEnumerable<TripDTO>> Trips([Service] TripsNocoDBService tripsService)
    => await tripsService.GetTripsAsync();
>>>>>>> e8eeb8f (Feat(payments): add trip create/update API and UI)
}
