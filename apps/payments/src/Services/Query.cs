using System.Text.Json.Nodes;
using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Data.Contracts.Models;

public class Query
{
  public async Task<IEnumerable<Transaction>> Transactions([Service] ITransactionsService transactionsService)
    => await transactionsService.GetTransactionsAsync();
}
