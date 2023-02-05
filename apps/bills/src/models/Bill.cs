namespace payobills.bills.models;

public class Bill
{
    public Guid Id { get; set; }
    public string? Name { get; set; } 
    public int BillingDate { get; set; }
    public int PayByDate { get; set; }
    public int LatePayByDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}