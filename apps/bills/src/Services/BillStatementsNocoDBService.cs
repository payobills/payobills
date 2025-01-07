using System.Text.Json;
using AutoMapper;
using HotChocolate.Execution;
using Payobills.Bills.Data.Contracts;
using Payobills.Bills.Data.Contracts.Models;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;
using Payobills.NocoDB;

namespace Payobills.Bills.Services;

public class BillStatementsNocoDBService : IBillStatementsService
{
    private readonly NocoDBClientService nocoDBClientService;
    private readonly IMapper mapper;

    public const string BILLS_NOCODB_FIELDS = "*";

    public BillStatementsNocoDBService(NocoDBClientService nocoDBClientService, IMapper mapper)
    {
        this.nocoDBClientService = nocoDBClientService;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<BillStatementDTO>> GetBillStatementsAsync(string billId)
    {
        var page = await nocoDBClientService.GetRecordsPageAsync<BillStatement>(
            "payobills",
            "bill-statements",
            "*",
            $"(nc_14ri__bills_id,eq,{billId})"
        );

        return (page?.List ?? []).Select(p => new BillStatementDTO(p));
    }

    public async Task<BillStatementDTO?> GetBillStatementByIdAsync(string billStatementId)
    {
        var billStatement = await nocoDBClientService.GetRecordByIdAsync<BillStatement>(
            billStatementId,
            "payobills",
            "bill-statements",
            "*"
        );

        return billStatement is not null ? new BillStatementDTO(billStatement) : null;
    }
}
