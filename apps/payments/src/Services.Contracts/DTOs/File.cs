using HotChocolate.ApolloFederation.Types;
using HotChocolate.Types.Relay;

namespace Payobills.Payments.Services.Contracts.DTOs;

[ExtendServiceType]
public class File
{
    [ID]
    [Key]
    public string Id { get; set; }
}
