using System.Text.Json.Serialization;

namespace Payobills.BillsParser.Data.Contracts.Models;

public class TransactionInput
{
    public string? Merchant { get; set; }
    public string? Currency { get; set; }
    public double? Amount { get; set; }
    public required string BackDateString { get; set; }
    public DateTime? BackDate { get; set; }

    public Dictionary<string, string> Metadata { get; set; } = new();
    public string ParseStatus { get; set;} = "NotStarted";

    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private DateTime updatedAt = DateTime.UtcNow;

    public string OcrId { get; set; } = string.Empty;

    public string Tags { get; set; } = string.Empty;

    public TransactionBillInput Bill = new();
}
