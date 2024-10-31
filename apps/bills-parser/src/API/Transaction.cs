using System.Text.Json.Serialization;

namespace Payobills.BillsParser.Data.Contracts.Models;

public class Transaction
{
    public long Id { get; set; }
    public string? Merchant { get; set; }
    public string? Currency { get; set; }
    public double? Amount { get; set; }
    public required string BackDateString { get; set; }
    public DateTime? BackDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Dictionary<string, string> Metadata { get; set; } = new();

    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private DateTime updatedAt;

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

    public string OcrId { get; set; } = string.Empty;

    public string Tags { get; set; } = string.Empty;
}
