using payobills.bills.svc;

public class HelperDateTimeService : IDateTimeService
{
    public DateTime UtcNow => new DateTime(2022, 1, 1);
}