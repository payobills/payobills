using Payobills.Bills.Data.Contracts.Models;

namespace Payobills.Bills.Services.Contracts.DTOs;

public class BillStatementDTO
{
    public BillStatementDTO(BillStatement from)
    {
        Id = from.Id.ToString();
        StartDate = from.StartDate?.ToString();
        EndDate = from.EndDate?.ToString();
        Notes = from.Notes ?? string.Empty;
        CreatedAt = from.CreatedAt;
        UpdatedAt = from.UpdatedAt;
    }

    public string Id { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}