using Payobills.Payments.Data.Contracts.Models;

namespace Payobills.Payments.Services.Contracts.DTOs
{
    public class TransactionDTO : Transaction
    {
        public TransactionDTO(Transaction parent)
        {
           Id = parent.Id;
           Merchant = parent.Merchant;
           Currency = parent.Currency;
           Amount = parent.Amount;
           BackDateString = parent.BackDateString;
           TransactionText = parent.TransactionText;
           BackDate = parent.BackDate;
           CreatedAt = parent.CreatedAt;
           UpdatedAt = parent.UpdatedAt;
           Tags = parent.Tags.Split(",");
        }

        public new IEnumerable<string> Tags { get => base.Tags?.Split(',') ?? []; set => base.Tags = string.Join(',', value); }
    }
}