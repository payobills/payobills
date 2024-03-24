using System.Text.Json;
using AutoMapper;
using HotChocolate.Execution;
using Payobills.Bills.Data.Contracts;
using Payobills.Bills.Data.Contracts.Models;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;
using Payobills.Bills.NocoDB;

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

    public async Task<BillDTO> AddBillAsync(CreateBillDTO dto)
    {
        var addedBill = await nocoDBClientService.CreateRecordAsync<CreateBillDTO, Bill>(
            "payobills",
            "bills",
            dto
        );

        var billDTO = mapper.Map<BillDTO>(addedBill);
        return billDTO;
    }

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

    public async Task<BillDTO?> GetBillByIdAsync(string id)
    {
        var bill = await nocoDBClientService.GetRecordByIdAsync<Bill>(
            id,
            "payobills",
            "bills",
            "Id,Name,BillingDate,LatePayByDate,CreatedAt,UpdatedAt,PayByDate"
        );
        var billDTO = bill is not null ? mapper.Map<BillDTO>(bill) : null;
        return billDTO;
    }

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