namespace Payobills.Payments.Services.Contracts.DTOs;

public class TransactionStatsFiltersInput
{
    public string? FromDate { get; set; }  // ISO 8601 date-time string e.g. "2025-04-15T00:00:00Z"
}
