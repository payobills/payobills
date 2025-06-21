using System.Text.Json.Serialization;

namespace Payobills.Bills.Data.Contracts.Models;

public class Edges
{
    public List<string> PaymentIDs { get; set; } = [];
}
