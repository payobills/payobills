namespace Payobills.Bills.Services.Contracts.DTOs;

public class PaymentDTO
{
    public required string Id { get; set; }
    public required Range<DateTime?> BillingPeriod { get; set; }
    public double? Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
