using System.Text.Json.Serialization;
using HotChocolate;
using Payobills.Payments.Data.Contracts.Models;

namespace Payobills.Payments.Services.Contracts.DTOs
{
    public class TransactionUpdateDTO
    {
        public string? Merchant { get; set; }
        public string? Currency { get; set; }
        public double? Amount { get; set; }
        public string? Notes { get { return notes; } set { notes = value ?? string.Empty; } }
        private string notes = string.Empty;
        public string? BackDateString { get; set; } = string.Empty;
        public DateTime? BackDate { get; set; }
    }
}