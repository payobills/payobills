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
            Tags = parent.Tags?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? [];
            Notes = parent.Notes;
        }

        public new IEnumerable<string> Tags { get; set; } = [];
    }
}