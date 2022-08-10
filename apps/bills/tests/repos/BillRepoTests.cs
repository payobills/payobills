namespace payobills.bills.tests.repos;

using Microsoft.EntityFrameworkCore;
using payobills.bills.dtos;
using payobills.bills.models;
using payobills.bills.repos;
using payobills.bills.data;
using Snapshooter.Xunit;

public class BillRepoTests
{
    private readonly BillRepo billRepo;

    public BillRepoTests()
    {
        var billsContextOptionsBuilder = new DbContextOptionsBuilder<BillsContext>();
        billsContextOptionsBuilder.UseMySql(
            "Server=localhost",
            new MySqlServerVersion(Environment.GetEnvironmentVariable("MYSQL_SERVER_VERSION"))
        );

        var billsContext = new BillsContext(billsContextOptionsBuilder.Options);

        this.billRepo = new BillRepo(
            new HelperGuidService(),
            new HelperDateTimeService(),
            billsContext
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