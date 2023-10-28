using Payobills.Bills.Services.Contracts;

namespace Payobills.Bills.Services;

public class GuidService : IGuidService
{
    public Guid NewGuid() => Guid.NewGuid();
}
