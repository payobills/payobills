using payobills.bills.dtos;
using payobills.bills.models;
using payobills.bills.repos;

namespace payobills.bills.svc;

public class BillService : IBillService
{
  private readonly BillRepo billRepo;

  public BillService(BillRepo billRepo) { this.billRepo = billRepo; }

  public Task<Bill> AddBillAsync(BillDto dto)
  {
    var bill = billRepo.AddBillAsync(dto);
    return bill;
  }
}