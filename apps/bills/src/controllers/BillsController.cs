using Microsoft.AspNetCore.Mvc;
using payobills.bills.dtos;
using payobills.bills.models;
using payobills.bills.svc;

namespace payobills.bills.controllers;

[Route("api/[controller]")]
[ApiController]
public class BillsController : ControllerBase
{
    private readonly IBillsService billService;

    public BillsController(IBillsService billService)
    {
        this.billService = billService;
    }

    [HttpPost]
    public async Task<Bill> AddBillAsync([FromBody] BillDto dto)
      => await this.billService.AddBillAsync(dto);

    [HttpGet]
    public Task<IEnumerable<Bill>> GetBillsAsync()
      => this.billService.GetBillsAsync();
}