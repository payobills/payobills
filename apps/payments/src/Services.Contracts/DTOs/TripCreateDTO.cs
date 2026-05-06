namespace Payobills.Payments.Services.Contracts.DTOs;

public class TripCreateDTO
{
    public string Title { get; set; } = string.Empty;
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}
