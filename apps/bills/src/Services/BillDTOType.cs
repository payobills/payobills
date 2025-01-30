using System.Text.Json.Nodes;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;
using HotChocolate.ApolloFederation.Resolvers;
using HotChocolate.Types.Relay;
using HotChocolate.ApolloFederation.Types;
using Payobills.Bills.Data.Contracts.Models;

public class BillDTOType : ObjectType<BillDTO>
// public class BillDTOType(IBillsService billsService) : ObjectType<BillDTO>
{
    // private readonly IBillsService billsService = billsService;

    protected override void Configure(IObjectTypeDescriptor<BillDTO> descriptor)
    {
        descriptor.Field(bill => bill.Id).ID();

        descriptor.Key("id").ResolveReferenceWith(_ => ResolveByIdAsync(default!));
       
    }

    [ReferenceResolver]
    private static async Task<BillDTO?> ResolveByIdAsync(
            // Represents the value that would be in the Id property of a Product
            string id)
    {
        // if bill
        // return await this.billsService.GetBillByIdAsync(id);
        return await Task.FromResult(new BillDTO
        {
            Id = id
        });
    }
}
