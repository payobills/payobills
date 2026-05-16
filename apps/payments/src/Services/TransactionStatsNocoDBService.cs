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

        var rows = page?.List ?? Enumerable.Empty<Dictionary<string, JsonElement>>();

        int GetCount(string status) => rows
            .Where(r => r.TryGetValue("ParseStatus", out var v) && v.GetString() == status)
            .Select(r => r.TryGetValue("count", out var c) ? c.GetInt32() : 0)
            .FirstOrDefault();

        var total = rows
            .Select(r => r.TryGetValue("count", out var c) ? c.GetInt32() : 0)
            .Sum();

        return new List<TransactionStatDTO>
        {
            new TransactionStatDTO { Stat = "total",      Value = total.ToString() },
            new TransactionStatDTO { Stat = "completed",  Value = GetCount("Completed").ToString() },
            new TransactionStatDTO { Stat = "notStarted", Value = GetCount("NotStarted").ToString() },
            new TransactionStatDTO { Stat = "pending",    Value = GetCount("Pending").ToString() },
            new TransactionStatDTO { Stat = "failed",     Value = GetCount("Failed").ToString() },
        };
    }
}
