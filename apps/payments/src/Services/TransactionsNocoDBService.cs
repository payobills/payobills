using System.Text.Json;
using AutoMapper;
using HotChocolate.Execution;
using Payobills.Payments.Data.Contracts.Models;
using Payobills.Payments.Services.Contracts;
using Payobills.Payments.NocoDB;
using HotChocolate.Data.Sorting;
using Payobills.Payments.Services.Contracts.DTOs;
using System.Net.WebSockets;

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

    public async Task<TransactionDTO> GetTransactionByIDAsync(string id)
    {
        var transaction = await nocoDBClientService.GetRecordByIdAsync<Transaction>(
            id,
            "payobills",
            "transactions",
            TRANSACTIONS_NOCODB_FIELDS,
            "w=(BackDate,isnotblank)&sort=-BackDate"
        );

        return mapper.Map<TransactionDTO>(transaction);
    }

    public async Task<TransactionDTO> SetTransactionTags(string id, string tags)
    {
        var updatedTransaction = await nocoDBClientService.UpdateRecordAsync<TransactionTagUpdateDTO, Transaction>(
             id,
             "payobills",
             "transactions",
             new TransactionTagUpdateDTO { Tags = tags }
         );

        return mapper.Map<TransactionDTO>(updatedTransaction);
    }

    public async Task<IEnumerable<TransactionTagDTO>> GetTransactionTagsAsync()
    {
        // TODO: Paginate over projects to search as NocoDB Meta APIs don't have project search API
        var getProjectsMetaUrl = "api/v1/db/meta/projects/";
        var projects = await nocoDBClientService.GetMetaResourceDataAsync<NocoDBPage<NocoDBMetaResourceIdWithTitle>>(getProjectsMetaUrl);
        var projectId = projects.List.Where(p => p.Title == "payobills").FirstOrDefault()?.Id ??
        throw new Exception("Could not find project");

        var getTablesMetaUrl = $"api/v1/db/meta/projects/{projectId}/tables";
        var tables = await nocoDBClientService.GetMetaResourceDataAsync<NocoDBPage<NocoDBMetaResourceIdWithTitle>>(getTablesMetaUrl);
        var tableId = tables.List.Where(p => p.Title == "transactions").FirstOrDefault()?.Id ??
        throw new Exception("Could not find table");

        var getTableDataMetaUrl = $"api/v1/db/meta/tables/{tableId}";
        var tableData = await nocoDBClientService.GetMetaResourceDataAsync<NocoDBTable>(getTableDataMetaUrl);
        var tagsColumn = tableData?.Columns.Where(c => c.Title == "Tags").FirstOrDefault() ?? throw new Exception("Could not find tags column");
        var tags = tagsColumn.ColOptions.Deserialize<NocoDBColOptions>()?.Options ?? [];

        return tags.Select(t => new TransactionTagDTO { Id = t.Id, Title = t.Title });
    }
}