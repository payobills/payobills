using System.Text.Json.Serialization;
using Payobills.Payments.Data.Contracts.Models;

namespace Payobills.Payments.Services.Contracts.DTOs
{
    public class TransactionUpdateInputDTO
    {
        public string? Merchant { get; set; }
        public string? Currency { get; set; }
        public double? Amount { get; set; }
        public string? Notes { get; set; }
    }
}