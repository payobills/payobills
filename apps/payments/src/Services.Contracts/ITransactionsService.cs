using HotChocolate.Data.Sorting;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services.Contracts;

public interface ITransactionsService
{
  Task<IEnumerable<TransactionDTO>> GetTransactionsAsync(SortInputType<TransactionDTO> order);
  Task<IEnumerable<TransactionDTO>> GetTransactionsByYearAndMonthAsync(int year, int month);
  Task<TransactionDTO> GetTransactionByIDAsync(string id);
  Task<TransactionDTO> SetTransactionTags(string id, string tags);
  Task<IEnumerable<TransactionTagDTO>> GetTransactionTagsAsync();
  Task<TransactionDTO> UpdateTransactionAsync(string id, TransactionUpdateDTO updateDTO);
}