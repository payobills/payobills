using System.Text.Json.Serialization;

namespace Payobills.Payments.Data.Contracts.Models;

public class Transaction
{
    public long Id { get; set; }
    public string? Merchant { get; set; }
    public string? Currency { get; set; }
    public double? Amount { get; set; }
    public required string BackDateString { get; set; }
}