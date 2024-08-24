using Payobills.Payments.Services.Contracts;

namespace Payobills.Payments.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
