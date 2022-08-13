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
    billsContextOptionsBuilder.UseSqlite("Data Source=payobills.bills.test.sqlite");

    var billsContext = new BillsContext(billsContextOptionsBuilder.Options);
    billsContext.Database.EnsureCreated();

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
    addedBill.Id = Guid.Empty;

    Snapshot.Match(addedBill);
  }
}