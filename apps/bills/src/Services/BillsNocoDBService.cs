using System.Text.Json;
using AutoMapper;
using HotChocolate.Execution;
using Payobills.Bills.Data.Contracts;
using Payobills.Bills.Data.Contracts.Models;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;
using Payobills.Bills.NocoBD;

namespace Payobills.Bills.Services;

public class BillsNocoDBService : IBillsService
{
    private readonly NocoDBClientService nocoDBClientService;
    private readonly IMapper mapper;

    public BillsNocoDBService(NocoDBClientService nocoDBClientService, IMapper mapper)
    {
        this.nocoDBClientService = nocoDBClientService;
        this.mapper = mapper;
    }

    public Task<BillDTO> AddBillAsync(CreateBillDTO dto) => throw new NotImplementedException();
    // {
    //     var billToAdd = mapper.Map<Bill>(dto);
    //     var bill = await billRepo.AddBillAsync(billToAdd);
    //     var billDTO = mapper.Map<BillDTO>(bill);
    //     return billDTO;
    // }

    public async Task<IEnumerable<BillDTO>> GetBillsAsync()
    {
        var page = await nocoDBClientService.GetRecordsPageAsync<Bill>(
            "payobills",
            "bills",
            "Id,Name,BillingDate,LatePayByDate,CreatedAt,UpdatedAt,PayByDate"
        );
        var bills = mapper.Map<List<BillDTO>>(page?.List);
        return bills;
    }

    public Task<BillDTO?> GetBillByIdAsync(Guid id) => throw new NotImplementedException();
    // {
    //     var bills = billRepo.GetBillByIdAsync(id);
    //     var billDTO = bills.Any() ? mapper.Map<BillDTO>(bills.First()) : null;
    //     return Task.FromResult(billDTO);
    // }

    public Task<PaymentDTO> MarkPaymentForBillAsync(MarkPaymentForBillDTO dto) => throw new NotImplementedException();
    // {
    //     var today = DateTime.SpecifyKind(DateTime.Today.AddDays(-DateTime.Today.Day), DateTimeKind.Utc);
    //     var billPeriodStart = today.Subtract(TimeSpan.FromDays(31));
    //     var billPeriodEnd = today;

    //     var bill = billRepo.GetBillByIdAsync(dto.Id).FirstOrDefault();

    //     if (bill is null) throw new ArgumentNullException(nameof(bill), $"The bill with {dto.Id} was not found.");

    //     var existingPayment = bill.Payments
    //         .Where(p => p.BillPeriodStart == billPeriodStart && p.BillPeriodEnd == billPeriodEnd)
    //         .FirstOrDefault();

    //     if (existingPayment is not null) return mapper.Map<PaymentDTO>(existingPayment);

    //     var payment = new BillPayment
    //     {
    //         Id = Guid.NewGuid(),
    //         BillPeriodStart = billPeriodStart,
    //         BillPeriodEnd = billPeriodEnd,
    //     };

    //     bill.Payments.Add(payment);
    //     await billRepo.UpdateBillAsync(bill);

    //     return mapper.Map<PaymentDTO>(payment);
    // }
}