using HotChocolate.Data.Sorting;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services.Contracts;

public interface ITransactionsService
{
  Task<IEnumerable<TransactionDTO>> GetTransactionsAsync(SortInputType<TransactionDTO> order, TransactionFiltersInput? filters = null!);
  Task<IEnumerable<TransactionDTO>> GetTransactionsByIDsAsync(IEnumerable<string> ids);
  Task<IEnumerable<TransactionDTO>> GetTransactionsByYearAndMonthAsync(int year, int month);
  Task<TransactionDTO> GetTransactionByIDAsync(string id);
  Task<TransactionDTO> SetTransactionTags(string id, string tags);
  Task<IEnumerable<TransactionTagDTO>> GetTransactionTagsAsync();
  Task<TransactionDTO> UpdateTransactionAsync(string id, TransactionUpdateDTO updateDTO);
  Task<TransactionDTO> AddTransactionAsync(TransactionAddDTO addDTO);
  Task<IEnumerable<Contracts.DTOs.File>> SyncTransactionReceiptsAsync(TransactionReceiptsSyncInput input);
}
