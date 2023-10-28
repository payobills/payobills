namespace Payobills.Bills.Services.Contracts.DTOs;

public class CreateBillDTO
{
    public string Name { get; set; } = string.Empty;
    public int BillingDate { get; set; }
    public int PayByDate { get; set; }
    public int LatePayByDate { get; set; }
}