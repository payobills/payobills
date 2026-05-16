using System.Text.Json;
using Payobills.Payments.NocoDB;
using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Services.Contracts.DTOs;

namespace Payobills.Payments.Services;

public class TransactionStatsNocoDBService : ITransactionStatsService
{
    private readonly NocoDBClientService nocoDBClientService;

    public TransactionStatsNocoDBService(NocoDBClientService nocoDBClientService)
    {
        this.nocoDBClientService = nocoDBClientService;
    }

    public async Task<IEnumerable<TransactionStatDTO>> GetTransactionStatsAsync(TransactionStatsFiltersInput? filters)
    {
        var filterUrlParam = string.IsNullOrEmpty(filters?.FromDate)
            ? string.Empty
            : $"where=(PaidAt,gte,{filters.FromDate})";

        var page = await nocoDBClientService.GetGroupByAsync<Dictionary<string, JsonElement>>(
            "payobills",
            "transactions",
            "ParseStatus",
            filterUrlParam
        );

        var rows = (page?.List ?? Enumerable.Empty<Dictionary<string, JsonElement>>()).ToList();

        int CountWhere(IEnumerable<string> statuses)
        {
            var set = new HashSet<string>(statuses, StringComparer.OrdinalIgnoreCase);
            return rows
                .Where(r => r.TryGetValue("ParseStatus", out var v) && set.Contains(v.GetString() ?? ""))
                .Sum(r => r.TryGetValue("count", out var c) ? c.GetInt32() : 0);
        }

        var total = rows.Sum(r => r.TryGetValue("count", out var c) ? c.GetInt32() : 0);

        return new List<TransactionStatDTO>
        {
            new TransactionStatDTO { Stat = "total",      Value = total.ToString() },
            new TransactionStatDTO { Stat = "completed",  Value = CountWhere(new[] { "ParsedV1", "Parsed", "OcrParsedV1" }).ToString() },
            new TransactionStatDTO { Stat = "notStarted", Value = CountWhere(new[] { "NotStarted" }).ToString() },
            new TransactionStatDTO { Stat = "pending",    Value = CountWhere(new[] { "Parsing" }).ToString() },
            new TransactionStatDTO { Stat = "failed",     Value = CountWhere(new[] { "FailedV1", "OcrFailedV1" }).ToString() },
        };
    }
}
