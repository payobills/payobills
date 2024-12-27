using System.Text.Json.Serialization;

namespace Payobills.Payments.Data.Contracts.Models;

public class Transaction
{
    public long Id { get; set; }
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
    public string Tags { get; set; } = string.Empty;

    [JsonPropertyName("bills")]
    public Bill Bill { get; set; } = default!;
}
