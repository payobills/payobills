using Payobills.Bills.Data.Contracts.Models;

namespace Payobills.Bills.Services.Contracts.DTOs;

public class AddBillStatementDTO
{
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public BillConnectionDTO Bill { get; set; } = default!;
}

