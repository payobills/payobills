using System.Net;
using System.Text.Json;
using AutoMapper;
using Payobills.Payments.Data.Contracts.Models;
using Payobills.Payments.Services.Contracts;
using Payobills.Payments.NocoDB;
using HotChocolate.Data.Sorting;
using Payobills.Payments.Services.Contracts.DTOs;
using Payobills.Payments.RabbitMQ;
using Microsoft.Extensions.Options;
using System.Data;

namespace Payobills.Payments.Services;

public class TransactionsNocoDBService : ITransactionsService
{
    private readonly NocoDBClientService nocoDBClientService;
    private readonly PayobillsNocoDBOptions nocoDBOptions;
    private readonly IMapper mapper;
    private readonly RabbitMQService rabbitMQService;

    public const string TRANSACTIONS_NOCODB_FIELDS = "*";

    public TransactionsNocoDBService(
        NocoDBClientService nocoDBClientService,
        IOptions<PayobillsNocoDBOptions> nocoDBOptions,
        RabbitMQService rabbitMQService,
        IMapper mapper)
    {
        this.nocoDBClientService = nocoDBClientService;
        this.nocoDBOptions = nocoDBOptions.Value;
        this.mapper = mapper;
        this.rabbitMQService = rabbitMQService;
    }

    public async Task<IEnumerable<TransactionDTO>> GetTransactionsAsync(SortInputType<TransactionDTO> _, TransactionFiltersInput? filters = null!)
    {
        // Build URL params with sorting and filtering
        // -> Add OcrID only if provided
        // -> Add SearchTerm only if provided (searches Merchant, TransactionText, Notes, Amount)
        // -> Add Tags only if provided 

        var sortUrlParam = "s=-PaidAt";
        var filterUrlParam = string.Join("~and", new List<string>{
            string.IsNullOrEmpty(filters?.OcrId) ? string.Empty : $"(OcrId,eq,{filters.OcrId})",
            string.IsNullOrEmpty(filters?.SearchTerm) ? string.Empty : $"((Id,like,%{filters.SearchTerm}%)~or(Merchant,like,%{filters.SearchTerm}%)~or(TransactionText,like,%{filters.SearchTerm}%)~or(Notes,like,%{filters.SearchTerm}%){(filters.SearchTerm.All(char.IsDigit) ? $"~or(Amount,eq,{filters.SearchTerm})" : string.Empty)})",
            filters?.Tags.Length > 0 ? string.Join("~or", filters.Tags.Select(tag => $"(Tags,like,{WebUtility.UrlEncode(tag)})")) : string.Empty
            }.Where(p => !string.IsNullOrEmpty(p))
        );

        var urlParams = string.Join("&", new List<string> {
            sortUrlParam,
            filterUrlParam != string.Empty ? $"w={filterUrlParam}" : string.Empty
        }.Where(p => !string.IsNullOrEmpty(p))
        );

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
        var transactionJsonString = await nocoDBClientService.GetRecordByIdAsync(
            id,
            "payobills",
            "transactions",
            TRANSACTIONS_NOCODB_FIELDS,
            "w=(BackDate,isnotblank)"
        );

        var transactionReceiptIds = parseFileIdsFromTransactionJsonString(transactionJsonString);
        var transaction = await nocoDBClientService.ParseJsonToNocoDBRecordAsync<Transaction>(transactionJsonString);

        transaction.Receipts = transactionReceiptIds
            .Select(receiptId => new Data.Contracts.Models.File { Id = receiptId });

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

        if (transactionUpdateResult.ParseStatus == "NotStarted")
        {
            await rabbitMQService.PublishMessageAsync(
                            "payobills.transaction-parsing",
                            JsonSerializer.Serialize(new
                            {
                                TransactionId = transactionUpdateResult.Id,
                            })
                        );
        }

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

    private int[] parseFileIdsFromTransactionJsonString(string? transactionJsonString)
    {
        if (string.IsNullOrEmpty(transactionJsonString))
        {
            return [];
        }

        using JsonDocument doc = JsonDocument.Parse(transactionJsonString);
        JsonElement root = doc.RootElement;

        var transactionFileIds = new List<int>();

        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in property.Value.EnumerateArray())
                {
                    if (item.TryGetProperty("files", out JsonElement filesProperty))
                    {
                        transactionFileIds.Add(filesProperty.GetProperty("Id").GetInt32());
                    }
                }
            }
        }

        return [.. transactionFileIds];
    }

    public async Task<IEnumerable<Contracts.DTOs.File>> SyncTransactionReceiptsAsync(TransactionReceiptsSyncInput input)
    {
        var transactionRecord = await nocoDBClientService.GetRecordByIdAsync<Transaction>(
                   input.TransactionID,
                   "payobills",
                   "transactions",
                   "*"
               );

        var currentReceiptsPage = await nocoDBClientService.GetManyToManyLinkRecordsAsync<IdDTO<int>>(
            "payobills",
            "transactions",
            transactionRecord.Id.ToString(),
            "Receipts"
        );

        // Detach all existing transaction receipts
        await nocoDBClientService.LinkManyToManyRecordsAsync(
            "payobills",
            "transactions",
            transactionRecord.Id.ToString(),
             "Receipts",
            currentReceiptsPage?.List?.Select(r => r.Id.ToString()) ?? [],
            NocoDbLinkOperation.Detach);

        // Attach new receipts based on TransactionID tags from files table
        var fileRecordsToAttach = await nocoDBClientService.GetRecordsPageAsync<Data.Contracts.Models.File>(
            "payobills",
            "files",
            "*",
            $"w=(Tags,like,\"TransactionID\":\"{input.TransactionID}\")"
        );

        await nocoDBClientService.LinkManyToManyRecordsAsync(
            "payobills",
            "transactions",
            transactionRecord.Id.ToString(),
            "Receipts",
            fileRecordsToAttach?.List.Select(f => f.Id.ToString()) ?? []);

        return fileRecordsToAttach?.List
            .Select(f => new Contracts.DTOs.File
            {
                Id = f.Id.ToString(),
            }) ?? [];
    }

    public async Task<TransactionTagDTO> AddOrUpdateTransactionTagAsync(TransactionTagAddOrUpdateDTO dto)
    {
        // Find out Transaction Table ID
        var getTablesMetaUrl = $"api/v2/meta/bases/{nocoDBOptions.BaseId}/tables";
        var tables = await nocoDBClientService.GetMetaResourceDataAsync<NocoDBPage<NocoDBMetaResourceIdWithTitle>>(getTablesMetaUrl);
        var tableId = tables.List.FirstOrDefault(p => p.Title == "transactions")?.Id ?? throw new System.Collections.Generic.KeyNotFoundException("Could not find table");

        // Get Transaction Table Details to find out Tags column
        var transactionTableDetailUrl = $"api/v2/meta/tables/{tableId}";
        var transactionTableDetail = await nocoDBClientService.GetMetaResourceDataAsync<NocoDBTable>(transactionTableDetailUrl);

        // Get transaction Tags column and add new option
        var tagsColumn = transactionTableDetail.Columns.FirstOrDefault(p => p.Title == "Tags") ?? throw new System.Collections.Generic.KeyNotFoundException("Could not find Tags column");
        var (TagId, TagTitle) = await nocoDBClientService.UpdateMultiSelectColumnMetadata(tagsColumn, dto.Id, dto.Title);

        return new TransactionTagDTO
        {
             Id = TagId,
             Title  = TagTitle
        };
    }
}
