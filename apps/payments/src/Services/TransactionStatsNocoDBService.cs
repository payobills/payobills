using Payobills.Payments.Data.Contracts.Models;
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
            : $"w=(PaidAt,gte,{filters.FromDate})";

        var records = new List<Transaction>();
        var pageNumber = 1;
        const int pageSize = 1000;

        while (true)
        {
            var urlParams = string.Join("&", new[] { $"l={pageSize}", $"p={pageNumber}", filterUrlParam }
                .Where(p => !string.IsNullOrEmpty(p)));

            var page = await nocoDBClientService.GetRecordsPageAsync<Transaction>(
                "payobills",
                "transactions",
                "ParseStatus",
                urlParams
            );

            records.AddRange(page?.List ?? Enumerable.Empty<Transaction>());

            if (page?.PageInfo.IsLastPage != false) break;
            pageNumber++;
        }

        var grouped = records
            .GroupBy(t => t.ParseStatus)
            .ToDictionary(g => g.Key, g => g.Count());

        var total = records.Count;

        int Get(string key) => grouped.TryGetValue(key, out var v) ? v : 0;

        return new List<TransactionStatDTO>
        {
            new TransactionStatDTO { Stat = "total",      Value = total.ToString() },
            new TransactionStatDTO { Stat = "completed",  Value = Get("Completed").ToString() },
            new TransactionStatDTO { Stat = "notStarted", Value = Get("NotStarted").ToString() },
            new TransactionStatDTO { Stat = "pending",    Value = Get("Pending").ToString() },
            new TransactionStatDTO { Stat = "failed",     Value = Get("Failed").ToString() },
        };
    }
}
