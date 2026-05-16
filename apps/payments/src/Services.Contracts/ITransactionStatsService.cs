using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services.Contracts;

public interface ITransactionStatsService
{
    Task<IEnumerable<TransactionStatDTO>> GetTransactionStatsAsync(TransactionStatsFiltersInput? filters);
}
