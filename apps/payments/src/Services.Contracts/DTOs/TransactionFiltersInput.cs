namespace Payobills.Payments.Services.Contracts.DTOs;

public class TransactionFiltersInput
{
    public string? OcrId { get; set; }
    public string? SearchTerm { get; set; }
}
