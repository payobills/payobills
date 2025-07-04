using HotChocolate.ApolloFederation.Types;
using Payobills.Payments.Data.Contracts.Models;
using System.Text.Json.Serialization;
using HotChocolate;

namespace Payobills.Payments.Services.Contracts.DTOs
{
    public class TransactionDTO
    {
        [Key]
        public new long Id { get; set; }
        public string? Merchant { get; set; }
        public string? Currency { get; set; }
        public double? Amount { get; set; }
        public string Notes { get { return notes; } set { notes = value ?? string.Empty; } }
        private string notes = string.Empty;
        public string TransactionText { get; set; } = string.Empty;
        public string BackDateString { get; set; } = string.Empty;
        public string ParseStatus { get; set; } = string.Empty;
        public DateTime? BackDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        private DateTime updatedAt;
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName(nameof(updatedAt))]
        public string UpdatedAtString
        {
            get => updatedAt.ToString("O");
            set
            {
                updatedAt = string.IsNullOrWhiteSpace(value) ? CreatedAt : DateTime.Parse(value);
                UpdatedAt = updatedAt;
            }
        }
        [JsonPropertyName("bills")]
        public Bill Bill { get; set; } = default!;
        public IEnumerable<File> Receipts { get; set; } = [];
        public IEnumerable<string> Tags { get; set; } = [];
        public TransactionDTO(Transaction parent)
        {
            Id = parent.Id;
            Merchant = parent.Merchant;
            Currency = parent.Currency;
            Amount = parent.Amount;
            BackDateString = parent.BackDateString;
            TransactionText = parent.TransactionText;
            BackDate = parent.BackDate;
            PaidAt = parent.PaidAt;
            CreatedAt = parent.CreatedAt;
            UpdatedAt = parent.UpdatedAt;
            Tags = parent.Tags?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? [];
            Notes = parent.Notes;
            ParseStatus = parent.ParseStatus;
            Bill = parent.Bill;
            Receipts = parent.Receipts.Select(p => new File { Id = p.Id.ToString() });
        }
    }
}