namespace payobills.bills.svc;

public class GuidService : IGuidService
{
    public Guid NewGuid() => Guid.NewGuid();
}
