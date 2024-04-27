using Payobills.Bills.Services.Contracts.DTOs;

public class StatDTO
{
    public string Type { get; set; } = string.Empty;
    public IEnumerable<string> BillIds { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<Range<int>> DateRanges { get; set; } = Enumerable.Empty<Range<int>>();
}
