using System.Text.Json;
using AutoMapper;
using HotChocolate.Execution;
using Payobills.Bills.Data.Contracts;
using Payobills.Bills.Data.Contracts.Models;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;
using Payobills.Bills.NocoDB;

namespace Payobills.Bills.Services;

public class BillStatementsNocoDBService : IBillStatementsService
{
    private readonly NocoDBClientService nocoDBClientService;
    private readonly IMapper mapper;

    public const string  BILLS_NOCODB_FIELDS= "*";

    public BillStatementsNocoDBService(NocoDBClientService nocoDBClientService, IMapper mapper)
    {
        this.nocoDBClientService = nocoDBClientService;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<BillStatementDTO>> GetBillStatementsAsync()
    {
        var page = await nocoDBClientService.GetRecordsPageAsync<BillStatement>(
            "payobills",
            "bill-statements",
            BILLS_NOCODB_FIELDS
        );

        return (page?.List ?? []).Select(p => new BillStatementDTO(p));
    }
}