using Amazon.Runtime;
using HotChocolate.Data.Sorting;
using Payobills.Payments.Data.Contracts.Models;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services.Contracts;

public interface ITransactionsService
{
  Task<IEnumerable<TransactionDTO>> GetTransactionsAsync(SortInputType<TransactionDTO> order);
  Task<IEnumerable<TransactionDTO>> GetTransactionsByYearAndMonthAsync(int year, int month);
  Task<TransactionDTO> GetTransactionByIDAsync(string id);
  Task<TransactionDTO> SetTransactionTags(string id, string tags);
}