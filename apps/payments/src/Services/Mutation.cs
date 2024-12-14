using System.Text.Json.Nodes;
using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Data.Contracts.Models;
using HotChocolate.Types.Pagination;
using HotChocolate.Data.Sorting;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services;

public class Mutation
{
  public async Task<TransactionDTO> SetTransactionTags([Service] ITransactionsService transactionsService, string id, string tags)
    => await transactionsService.SetTransactionTags(id, tags);
}
