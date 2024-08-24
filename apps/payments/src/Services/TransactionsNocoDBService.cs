using System.Text.Json;
using AutoMapper;
using HotChocolate.Execution;
using Payobills.Payments.Data.Contracts.Models;
using Payobills.Payments.Services.Contracts;
using Payobills.Payments.NocoDB;

namespace Payobills.Payments.Services;

public class TransactionsNocoDBService : ITransactionsService
{
    private readonly NocoDBClientService nocoDBClientService;
    private readonly IMapper mapper;

    public const string  TRANSACTIONS_NOCODB_FIELDS= "Id,Amount,BackDateString,Merchant,Currency";

    public TransactionsNocoDBService(NocoDBClientService nocoDBClientService, IMapper mapper)
    {
        this.nocoDBClientService = nocoDBClientService;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync()
    {
        var page = await nocoDBClientService.GetRecordsPageAsync<Transaction>(
            "payobills",
            "transactions",
            TRANSACTIONS_NOCODB_FIELDS,
            "w=(ParseStatus,eq,ParsedV1)~and(BackDateString,isnot,null)"
        );

        return page?.List ?? Enumerable.Empty<Transaction>();
    }
}