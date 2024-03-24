using System.Text.Json.Serialization;

namespace Payobills.Bills.Services.Contracts.DTOs;

public record MarkPaymentForBillDTO
{
    public DateTime? BillPeriodStart { get; set; }
    public DateTime? BillPeriodEnd { get; set; }
    public double? Amount { get; set; }
    [JsonPropertyName("nc_14ri__bills_id")]
    public required string BillId { get; set; }
}