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

    public Task<BillDTO?> GetBillByIdAsync(Guid id)
    {
        var bills = billRepo.GetBillByIdAsync(id);
        var billDTO = bills.Any() ? mapper.Map<BillDTO>(bills.First()) : null;
        return Task.FromResult(billDTO);
    }
}