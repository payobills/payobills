using HotChocolate.Data.Sorting;
using Payobills.Payments.Data.Contracts.Models;

namespace Payobills.Payments.Services.Contracts;

public interface ITransactionsService
{
  Task<IEnumerable<Transaction>> GetTransactionsAsync(SortInputType<Transaction> order);
  Task<IEnumerable<Transaction>> GetTransactionsByYearAndMonthAsync(int year, int month);
}