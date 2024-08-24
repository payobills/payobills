using Payobills.Payments.Data.Contracts.Models;

namespace Payobills.Payments.Services.Contracts;

public interface ITransactionsService
{
  Task<IEnumerable<Transaction>> GetTransactionsAsync();
}