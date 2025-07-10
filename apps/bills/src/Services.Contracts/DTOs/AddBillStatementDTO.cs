using Payobills.Bills.Data.Contracts.Models;
using System.Text.Json.Serialization; 

namespace Payobills.Bills.Services.Contracts.DTOs;

public class AddBillStatementDTO
{
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public double? Amount { get; set; }
    public ConnectionDTO Bill { get; set; } = default!;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ConnectionDTO? File { get; set; } = null;
    public EdgesDTO? Edges { get; set; } = new();
}
