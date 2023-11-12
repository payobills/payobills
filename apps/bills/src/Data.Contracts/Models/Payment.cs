namespace Payobills.Bills.Data.Contracts.Models;

public class BillPayment
{
    public Guid Id { get; set; }
    public (DateTime Start, DateTime End) BillingPeriod { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
