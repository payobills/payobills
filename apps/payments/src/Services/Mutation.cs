using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services;

public class Mutation
{
  [GraphQLDeprecated("Use the transactionUpdate mutation instead")]
  public async Task<TransactionDTO> SetTransactionTags([Service] ITransactionsService transactionsService, string id, string tags)
    => await transactionsService.SetTransactionTags(id, tags);

  public async Task<TransactionDTO> TransactionUpdate([Service] ITransactionsService transactionsService, string id, TransactionUpdateDTO updateDTO)
    => await transactionsService.UpdateTransactionAsync(id, updateDTO);

  public async Task<TransactionDTO> TransactionAdd([Service] ITransactionsService transactionsService, TransactionAddDTO input)
    => await transactionsService.AddTransactionAsync(input);

  public async Task<IEnumerable<Contracts.DTOs.File>> TransactionReceiptsSync([Service] ITransactionsService transactionsService, TransactionReceiptsSyncInput input)
    => await transactionsService.SyncTransactionReceiptsAsync(input);

  public async Task<TransactionTagDTO> TransactionTagsAddOrUpdate([Service] ITransactionsService transactionsService, TransactionTagAddOrUpdateDTO input)
    => await transactionsService.AddOrUpdateTransactionTagAsync(input);
}
