using System.Text.Json.Serialization;

namespace Payobills.Bills.Services.Contracts.DTOs;

public record EdgesDTO
{
    public List<string> PaymentIds { get; set; } = [];
}