using HotChocolate.ApolloFederation.Types;
using HotChocolate.Types.Relay;
using Payobills.Bills.Data.Contracts.Models;

namespace Payobills.Bills.Services.Contracts.DTOs;

[ExtendServiceType]
public class File(FileModel from)
{
    [ID]
    [Key]
    public string Id { get; set; } = from.Id.ToString();
}
