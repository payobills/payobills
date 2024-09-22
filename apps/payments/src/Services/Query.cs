using System.Text.Json.Nodes;
using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Data.Contracts.Models;
using HotChocolate.Types.Pagination;
using HotChocolate.Data.Sorting;

namespace Payobills.Payments.Services;

public class Query
{
  [UsePaging]
  [UseSorting]
  public async Task<IEnumerable<Transaction>> Transactions([Service] ITransactionsService transactionsService)
    => await transactionsService.GetTransactionsAsync(null!);
}
