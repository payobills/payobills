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

    public async Task<IEnumerable<TransactionDTO>> GetTransactionsAsync(SortInputType<TransactionDTO> _, TransactionFiltersInput? filters = null!)
    {
        var ocrId = filters?.OcrId ?? string.Empty;

        var sortUrlParam = "s=-PaidAt";
        var filterUrlParam = string.IsNullOrEmpty(ocrId) ? string.Empty : $"w=(OcrId,eq,{ocrId})";
        var urlParams = string.Join("&", sortUrlParam, filterUrlParam);

        var page = await nocoDBClientService.GetRecordsPageAsync<Transaction>(
            "payobills",
            "transactions",
            TRANSACTIONS_NOCODB_FIELDS,
            urlParams
        );

        return mapper.Map<List<TransactionDTO>>(page?.List ?? []);
    }

    public async Task<IEnumerable<TransactionDTO>> GetTransactionsByYearAndMonthAsync(int year, int month)
    {
        var page = await nocoDBClientService.GetRecordsPageAsync<Transaction>(
            "payobills",
            "transactions",
            TRANSACTIONS_NOCODB_FIELDS,
            $"l=1000&w=(PaidYear,eq,{year})~and(PaidMonth,eq,{month})&s=-PaidAt"
        );

        var transactions = (page?.List ?? []).Select(p => new TransactionDTO(p)
        {
            BackDateString = string.Empty,
            Notes = p.Notes
        });
        return transactions;
    }

    public async Task<TransactionDTO> GetTransactionByIDAsync(string id)
    {
        var transaction = await nocoDBClientService.GetRecordByIdAsync<Transaction>(
            id,
            "payobills",
            "transactions",
            TRANSACTIONS_NOCODB_FIELDS,
            "w=(BackDate,isnotblank)"
        );

        return new TransactionDTO(transaction!) { Notes = transaction!.Notes };
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

    public async Task<TransactionDTO> UpdateTransactionAsync(string id, TransactionUpdateDTO updateDTO)
    {
        var payloadSerializeOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        };

        var transactionUpdateResult = await nocoDBClientService.UpdateRecordAsync<TransactionUpdateDTO, Transaction>(
            id,
            "payobills",
            "transactions",
            updateDTO,
            payloadSerializeOptions);

        var mappedTransactionDTO = new TransactionDTO(transactionUpdateResult);
        return mappedTransactionDTO;
    }

    public async Task<TransactionDTO> AddTransactionAsync(TransactionAddDTO addDTO)
    {
        addDTO.SourceSystemID = "";
        addDTO.BackDateString = "";

        var addedTransaction = await nocoDBClientService.CreateRecordAsync<TransactionAddDTO, Transaction>(
            "payobills",
            "transactions",
            addDTO
        );

        var transactionDTO = new TransactionDTO(addedTransaction);
        return transactionDTO;
    }

    public async Task<IEnumerable<TransactionDTO>> GetTransactionsByIDsAsync(IEnumerable<string> ids)
    {
        var idFilterString = string.Join("~or", ids.Select(p => $"(Id,eq,{p})"));
        var page = await nocoDBClientService.GetRecordsPageAsync<Transaction>(
            "payobills",
            "transactions",
            TRANSACTIONS_NOCODB_FIELDS,
            $"&w={idFilterString}&s=-PaidAt"
        );

        return mapper.Map<List<TransactionDTO>>(page?.List ?? []);
    }
}
