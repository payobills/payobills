using HotChocolate.Data.Sorting;
using Payobills.Payments.Data.Contracts.Models;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services.Contracts;

public interface ITransactionsService
{
  Task<IEnumerable<TransactionDTO>> GetTransactionsAsync(SortInputType<TransactionDTO> order);
  Task<IEnumerable<TransactionDTO>> GetTransactionsByYearAndMonthAsync(int year, int month);
}