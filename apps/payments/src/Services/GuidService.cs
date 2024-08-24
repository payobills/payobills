using Payobills.Payments.Services.Contracts;

namespace Payobills.Payments.Services;

public class GuidService : IGuidService
{
    public Guid NewGuid() => Guid.NewGuid();
}
