using Microsoft.AspNetCore.Mvc;
using payobills.bills.dtos;
using payobills.bills.models;
using payobills.bills.svc;

namespace payobills.bills.controllers;

[Route("api/[controller]")]
[ApiController]
public class BillController : ControllerBase
{
  private readonly IBillService billService;

  public BillController(IBillService billService)
  {
    this.billService = billService;
  }

  [HttpPost]
  public async Task<Bill> AddBillAsync([FromBody] BillDto dto)
  {
    return await this.billService.AddBillAsync(dto);
  }
}