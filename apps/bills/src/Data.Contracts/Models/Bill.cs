namespace Payobills.Bills.Data.Contracts.Models;

public class Bill
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BillingDate { get; set; } = null;
    public string? PayByDate { get; set; } = null;
    public string? LatePayByDate { get; set; } = null;
    public List<BillPayment> Payments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}