using System.Text.Json.Serialization;

namespace Payobills.Bills.Data.Contracts.Models;

public class BillStatement
{
    public long Id { get; set; }
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public BillConnection Bill { get; set; } = default!;
    
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
