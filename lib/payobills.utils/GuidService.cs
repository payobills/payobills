namespace payobills.utils.svc;

public class GuidService : IGuidService
{
    public Guid NewGuid() => Guid.NewGuid();
}
