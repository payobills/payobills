using payobills.bills.svc;

public class HelperGuidService : IGuidService
{
    public Guid NewGuid() {
        return Guid.Empty;
    }
}