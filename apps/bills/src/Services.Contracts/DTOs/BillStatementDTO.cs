using Payobills.Bills.Data.Contracts.Models;

namespace Payobills.Bills.Services.Contracts.DTOs;

public class BillStatementDTO(BillStatement from)
{
    public string Id { get; set; } = from.Id.ToString();
    public string? StartDate { get; set; } = from.StartDate?.ToString();
    public string? EndDate { get; set; } = from.EndDate?.ToString();
    public string Notes { get; set; } = from.Notes ?? string.Empty;
    public DateTime CreatedAt { get; set; } = from.CreatedAt;
    public DateTime UpdatedAt { get; set; } = from.UpdatedAt;
    public BillConnection Bill { get; set; } = from.Bill;
    public DTOs.File? Statement { get; set; } = from.File is null ? null : new File(from.File);
}
