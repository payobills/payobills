namespace Payobills.Payments.Services.Contracts;

public interface IDateTimeService
{
    public DateTime UtcNow { get; }
}