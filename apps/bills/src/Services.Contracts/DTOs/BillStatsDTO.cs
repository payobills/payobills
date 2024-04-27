using Payobills.Bills.Services.Contracts.DTOs;

public class BillStatsDTO
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IEnumerable<StatDTO> Stats { get; set; } = Enumerable.Empty<StatDTO>();
}
