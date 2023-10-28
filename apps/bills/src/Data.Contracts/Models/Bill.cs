namespace Payobills.Bills.Data.Contracts.Models;

public class Bill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BillingDate { get; set; }
    public int PayByDate { get; set; }
    public int LatePayByDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}