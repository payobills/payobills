using System.Globalization;
using System.Text.Json.Serialization;

namespace Payobills.Bills.Data.Contracts.Models;

public class BillPayment
{
    public long Id { get; set; }
    public DateTime? BillPeriodStart { get; set; }
    public DateTime? BillPeriodEnd { get; set; }
    public double? Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("nc_14ri__bills_id")]
    public required long BillId { get; set; }
}
