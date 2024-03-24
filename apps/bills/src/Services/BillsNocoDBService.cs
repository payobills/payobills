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

    public const string  BILLS_NOCODB_FIELDS= "Id,Name,BillingDate,LatePayByDate,CreatedAt,UpdatedAt,PayByDate";

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
            BILLS_NOCODB_FIELDS
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
            $"{BILLS_NOCODB_FIELDS},Payments&nested[Payments][fields]=*"
        );
        var billDTO = bill is not null ? mapper.Map<BillDTO>(bill) : null;
        return billDTO;
    }

    public async Task<PaymentDTO> MarkPaymentForBillAsync(MarkPaymentForBillDTO dto)
    {
        var bill = await nocoDBClientService.GetRecordByIdAsync<Bill>(
            dto.BillId,
            "payobills", 
            "bills",
            $"{BILLS_NOCODB_FIELDS}&nested[Payments][fields]=*");

        if (bill is null) throw new ArgumentNullException(nameof(bill), $"The bill with {dto.BillId} was not found.");
        
        var markedPayment = await nocoDBClientService.CreateRecordAsync<MarkPaymentForBillDTO, BillPayment>(
            "payobills",
            "payments",
            dto
        );

        return mapper.Map<PaymentDTO>(markedPayment);
    }
}