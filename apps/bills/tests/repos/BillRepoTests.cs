namespace payobills.bills.tests.repos;

using payobills.bills.dtos;
using payobills.bills.models;
using payobills.bills.repos;
using payobills.bills.svc;
using Snapshooter.Xunit;

public class BillRepoTests
{
    private readonly BillRepo billRepo;

    public BillRepoTests()
    {
        this.billRepo = new BillRepo(
            new HelperGuidService(),
            new HelperDateTimeService()
        );
    }

    [Fact]
    public async Task ShouldCreateBill_SnapshotTest()
    {
        Bill addedBill;
        var billDto = new BillDto
        {
            Name = "Electricity - Home",
            BillingDate = 3,
            PayByDate = 17,
        };

        addedBill = await billRepo.AddBillAsync(billDto);

        Snapshot.Match(addedBill);
    }
}