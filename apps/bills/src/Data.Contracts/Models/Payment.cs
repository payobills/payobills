namespace Payobills.Bills.Data.Contracts.Models;

public class BillPayment
{
    public Guid Id { get; set; }
    public DateTime[] BillingPeriod => new DateTime[] {
        Start, End
    };
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
