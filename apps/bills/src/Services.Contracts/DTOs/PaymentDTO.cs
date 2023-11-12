namespace Payobills.Bills.Services.Contracts.DTOs;

public class PaymentDTO
{
    public Guid Id { get; set; }
    public DateTime[] BillingPeriod { get; set; } = Array.Empty<DateTime>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
