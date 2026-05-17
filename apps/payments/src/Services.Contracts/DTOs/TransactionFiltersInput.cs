namespace Payobills.Payments.Services.Contracts.DTOs;

public class TransactionFiltersInput
{
    public string? OcrId { get; set; }
    public string? SearchTerm { get; set; }
    public string[]? Tags {
      get { return tags ?? []; }
      set { tags = value; }
    }

    private string[]? tags;

    public DateTime? StartPaidAt { get; set; }
    public DateTime? EndPaidAt { get; set; }
}
