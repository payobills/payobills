using HotChocolate.Types;

namespace Payobills.Bills.Services.Contracts.DTOs;

public class BillDTO
{
    public required string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? BillingDate { get; set; }
    public int? PayByDate { get; set; }
    public int? LatePayByDate { get; set; }
    public bool IsEnabled { get; set; } = false;
    public List<PaymentDTO> Payments { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}