using AutoMapper;
using HotChocolate.Execution;
using Payobills.Bills.Data.Contracts;
using Payobills.Bills.Data.Contracts.Models;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;

namespace Payobills.Bills.Services;

public class BillsService : IBillsService
{
    private readonly IBillsRepo billRepo;
    private readonly IMapper mapper;

    public BillsService(IBillsRepo billRepo, IMapper mapper)
    {
        this.billRepo = billRepo;
        this.mapper = mapper;
    }

    public async Task<BillDTO> AddBillAsync(CreateBillDTO dto)
    {
        var billToAdd = mapper.Map<Bill>(dto);
        var bill = await billRepo.AddBillAsync(billToAdd);
        var billDTO = mapper.Map<BillDTO>(bill);
        return billDTO;
    }

    public Task<IEnumerable<BillDTO>> GetBillsAsync()
    {
        var bills = billRepo.GetBillsAsync();
        var billsDTOList = mapper.Map<IEnumerable<BillDTO>>(bills);
        return Task.FromResult(billsDTOList);
    }

    public Task<BillDTO?> GetBillByIdAsync(string id)
    {
        var bills = billRepo.GetBillByIdAsync(Guid.Parse(id));
        var billDTO = bills.Any() ? mapper.Map<BillDTO>(bills.First()) : null;
        return Task.FromResult(billDTO);
    }

    public async Task<PaymentDTO> MarkPaymentForBillAsync(MarkPaymentForBillDTO dto)
    {
        var today = DateTime.SpecifyKind(DateTime.Today.AddDays(-DateTime.Today.Day), DateTimeKind.Utc);
        var billPeriodStart = today.Subtract(TimeSpan.FromDays(31));
        var billPeriodEnd = today;

        var bill = billRepo.GetBillByIdAsync(dto.Id).FirstOrDefault();

        if (bill is null) throw new ArgumentNullException(nameof(bill), $"The bill with {dto.Id} was not found.");

        var existingPayment = bill.Payments
            .Where(p => p.BillPeriodStart == billPeriodStart && p.BillPeriodEnd == billPeriodEnd)
            .FirstOrDefault();

        if (existingPayment is not null) return mapper.Map<PaymentDTO>(existingPayment);

        var payment = new BillPayment
        {
            Id = Guid.NewGuid(),
            BillPeriodStart = billPeriodStart,
            BillPeriodEnd = billPeriodEnd,
        };

        bill.Payments.Add(payment);
        await billRepo.UpdateBillAsync(bill);

        return mapper.Map<PaymentDTO>(payment);
    }
}