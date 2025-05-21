using System.Text.Json.Nodes;
using Payobills.Bills.Services;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;
using Payobills.Bills.Data.Contracts.Models;
using HotChocolate.ApolloFederation.Resolvers;
using HotChocolate.Types.Relay;
using HotChocolate.ApolloFederation.Types;

public class BillDTOType : ObjectType<BillDTO>
{
    protected override void Configure(IObjectTypeDescriptor<BillDTO> descriptor)
    {
        descriptor.Field(bill => bill.Id).ID();
        descriptor.Key("id").ResolveReferenceWith(_ => ResolveByIdAsync(default!, default!));
    }

    [ReferenceResolver]
    private static Task<BillDTO?> ResolveByIdAsync(
        string id,
        BillDTODataLoader billDTODataLoader)
    {
        return billDTODataLoader.LoadAsync(id);
    }
}
