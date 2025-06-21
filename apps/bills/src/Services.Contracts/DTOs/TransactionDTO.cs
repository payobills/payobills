using HotChocolate.ApolloFederation.Types;
using HotChocolate.Types.Relay;

namespace Payobills.Bills.Services.Contracts.DTOs;

[ExtendServiceType]
public class TransactionDTO
{
    [ID]
    [Key]
    public required string Id { get; set; }
}

