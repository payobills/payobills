namespace Payobills.Bills.Data.Contracts.Models;

public class BillPayment
{
    public Guid Id { get; set; }
    public DateTime BillPeriodStart { get; set; }
    public DateTime BillPeriodEnd { get; set; }
    public double? Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
