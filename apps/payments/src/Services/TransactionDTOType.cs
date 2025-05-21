using System.Text.Json.Nodes;
using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Services.Contracts.DTOs;
using Payobills.Payments.Data.Contracts.Models;
using HotChocolate.ApolloFederation.Resolvers;
using HotChocolate.Types.Relay;
using HotChocolate.ApolloFederation.Types;
using Payobills.Bills.Services;

public class TransactionDTOType : ObjectType<TransactionDTO>
{
    protected override void Configure(IObjectTypeDescriptor<TransactionDTO> descriptor)
    {
        descriptor.Field(bill => bill.Id).ID();
        descriptor.Key("id").ResolveReferenceWith(_ => ResolveByIdAsync(default!, default!));
    }

    [ReferenceResolver]
    private static Task<TransactionDTO?> ResolveByIdAsync(
        string id,
        TransactionDTODataLoader billDTODataLoader)
    {
        return billDTODataLoader.LoadAsync(id);
    }
}