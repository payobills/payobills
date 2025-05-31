using HotChocolate.ApolloFederation.Types;

namespace Payobills.Bills.Services.Contracts.DTOs;

public class BillDTO
{
    [ID]
    public required string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? BillingDate { get; set; }
    public int? PayByDate { get; set; }
    public int? LatePayByDate { get; set; }
    public List<PaymentDTO> Payments { get; set; } = [];
    public bool IsEnabled { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
