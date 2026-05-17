namespace Payobills.Payments.Data.Contracts.Models;

public class Trip
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
