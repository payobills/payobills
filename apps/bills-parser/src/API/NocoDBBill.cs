using System.Text.Json.Serialization;

public class NocoDBBill
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? BillingDate { get; set; }
    public int? PayByDate { get; set; }
    public int? LatePayByDate { get; set; }
    public bool IsEnabled { get; set; } = false;

    public string? Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private DateTime updatedAt;

    [JsonPropertyName("updatedAt")]
    public string UpdatedAtString
    {
        get => updatedAt.ToString("O");
        set
        {
            updatedAt = string.IsNullOrWhiteSpace(value) ? CreatedAt : DateTime.Parse(value);
            UpdatedAt = updatedAt;
        }
    }
}