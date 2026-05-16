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
        var scope = filters?.Scope;
        var fromDate = filters?.FromDate;

        var tasks = new List<Task<IEnumerable<TransactionStatDTO>>>();

        if (scope is null or TransactionStatScope.Parse)
            tasks.Add(GetParseStatsAsync(fromDate));
        if (scope is null or TransactionStatScope.Tags)
            tasks.Add(GetTagStatsAsync(fromDate));
        if (scope is null or TransactionStatScope.Currency)
            tasks.Add(GetCurrencyStatsAsync(fromDate));

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r);
    }

    private async Task<IEnumerable<TransactionStatDTO>> GetParseStatsAsync(string? fromDate)
    {
        var filterUrlParam = string.IsNullOrEmpty(fromDate)
            ? string.Empty
            : $"where=(PaidAt,gte,{fromDate})";

        var page = await nocoDBClientService.GetGroupByAsync<Dictionary<string, JsonElement>>(
            "payobills", "transactions", "ParseStatus", filterUrlParam);

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

    private async Task<IEnumerable<TransactionStatDTO>> GetTagStatsAsync(string? fromDate)
    {
        string TaggedFilter() => string.IsNullOrEmpty(fromDate)
            ? "where=(Tags,isnotblank)"
            : $"where=(Tags,isnotblank)~and(PaidAt,gte,{fromDate})";

        string UntaggedFilter() => string.IsNullOrEmpty(fromDate)
            ? "where=(Tags,isblank)"
            : $"where=(Tags,isblank)~and(PaidAt,gte,{fromDate})";

        var taggedTask = nocoDBClientService.GetCountAsync("payobills", "transactions", TaggedFilter());
        var untaggedTask = nocoDBClientService.GetCountAsync("payobills", "transactions", UntaggedFilter());

        await Task.WhenAll(taggedTask, untaggedTask);

        return new List<TransactionStatDTO>
        {
            new TransactionStatDTO { Stat = "tagged",   Value = taggedTask.Result.ToString() },
            new TransactionStatDTO { Stat = "untagged", Value = untaggedTask.Result.ToString() },
        };
    }

    private async Task<IEnumerable<TransactionStatDTO>> GetCurrencyStatsAsync(string? fromDate)
    {
        var filterUrlParam = string.IsNullOrEmpty(fromDate)
            ? string.Empty
            : $"where=(PaidAt,gte,{fromDate})";

        var page = await nocoDBClientService.GetGroupByAsync<Dictionary<string, JsonElement>>(
            "payobills", "transactions", "Currency", filterUrlParam);

        var rows = page?.List ?? Enumerable.Empty<Dictionary<string, JsonElement>>();

        return rows
            .Where(r => r.TryGetValue("Currency", out var v) && !string.IsNullOrEmpty(v.GetString()))
            .Select(r => new TransactionStatDTO
            {
                Stat = $"currency_{r["Currency"].GetString()!.ToLowerInvariant()}",
                Value = (r.TryGetValue("count", out var c) ? c.GetInt32() : 0).ToString(),
            });
    }
}
