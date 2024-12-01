using System.Text.Json;
using AutoMapper;
using HotChocolate.Execution;
using Payobills.Payments.Data.Contracts.Models;
using Payobills.Payments.Services.Contracts;
using Payobills.Payments.NocoDB;
using HotChocolate.Data.Sorting;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services;

public class TransactionsNocoDBService : ITransactionsService
{
    private readonly NocoDBClientService nocoDBClientService;
    private readonly IMapper mapper;

    public const string TRANSACTIONS_NOCODB_FIELDS = "*";

    public TransactionsNocoDBService(NocoDBClientService nocoDBClientService, IMapper mapper)
    {
        this.nocoDBClientService = nocoDBClientService;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<TransactionDTO>> GetTransactionsAsync(SortInputType<TransactionDTO> order)
    {
        var page = await nocoDBClientService.GetRecordsPageAsync<Transaction>(
            "payobills",
            "transactions",
            TRANSACTIONS_NOCODB_FIELDS,
            "w=(BackDate,isnotblank)&sort=-BackDate"
        );

        return mapper.Map<List<TransactionDTO>>(page?.List ?? []);
    }

    public async Task<IEnumerable<TransactionDTO>> GetTransactionsByYearAndMonthAsync(int year, int month)
    {
        var page = await nocoDBClientService.GetRecordsPageAsync<Transaction>(
            "payobills",
            "transactions",
            TRANSACTIONS_NOCODB_FIELDS,
            $"l=1000&w=(ParseStatus,eq,ParsedV1)~and(BackDate,isnotblank)~and(BackDateYear,eq,{year})~and(BackDateMonth,eq,{month})&sort=-BackDate"
        );

        var transactions = (page?.List ?? []).Select(p => new TransactionDTO(p) { BackDateString = string.Empty });
        return transactions;
    }
}