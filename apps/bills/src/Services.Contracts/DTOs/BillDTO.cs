namespace Payobills.Bills.Services.Contracts.DTOs;

public class BillDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BillingDate { get; set; }
    public int PayByDate { get; set; }
    public int LatePayByDate { get; set; }
    public List<PaymentDTO> Payments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}