namespace Payobills.Bills.Services.Contracts;

public interface IDateTimeService
{
    public DateTime UtcNow { get; }
}