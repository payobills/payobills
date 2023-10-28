using Payobills.Bills.Services.Contracts;

namespace Payobills.Bills.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
