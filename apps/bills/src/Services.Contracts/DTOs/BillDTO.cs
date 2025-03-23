using System.ComponentModel.DataAnnotations;
using HotChocolate.ApolloFederation.Resolvers;
using HotChocolate.Types.Relay;

namespace Payobills.Bills.Services.Contracts.DTOs;

public class BillDTO
{
    // [ID]
    [Key]
    public required string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? BillingDate { get; set; }
    public int? PayByDate { get; set; }
    public int? LatePayByDate { get; set; }
    public bool IsEnabled { get; set; } = false;
    // public List<PaymentDTO> Payments { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // [ReferenceResolver]
    // private Task<BillDTO?> ResolveByIdAsync(
    //     // Represents the value that would be in the Id property of a Product
    //     string id)
    // {
    //     // return await billsService.GetBillByIdAsync(id);
    //     return Task.FromResult(new BillDTO
    //     {
    //         Id = id
    //     });
    // }
}