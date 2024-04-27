using Payobills.Bills.Services;
using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services.Contracts.DTOs;

public class StatsQueryService
{
    private readonly IBillsService billsService;

    public StatsQueryService(IBillsService billsService)
    {
        this.billsService = billsService;
    }

    public async Task<BillStatsDTO> BillStats(int year, int month)
    {
        var bills = await billsService.GetBillsAsync();
        var filteredBills = bills.Where(b => b.PayByDate.HasValue && b.BillingDate.HasValue);

        var startDate = new DateTime(
            year,
            month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc
        );

        var endDate = new DateTime(
            month == 12 ? year + 1 : year,
            month == 12 ? 1 : month + 1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc
        ).AddSeconds(-1);


        var billsWithRanges = filteredBills
            .Where(b => b.PayByDate.HasValue && b.BillingDate.HasValue)
            .Select(b =>
            {
                return (
                    Id: b.Id,
                    BillingRange: b.BillingDate > b.PayByDate ?
                        new Range<int>[] { new Range<int>(1, b.PayByDate.Value), new Range<int>(b.BillingDate.Value, endDate.Day) } :
                        new Range<int>[] { new Range<int>(b.BillingDate!.Value, b.PayByDate!.Value) }
                    );
            });


        var overlaps = new Dictionary<int, int>();
        var currentStart = 1;

        var fullPaymentDates = new List<Range<int>>();

        var inFullPayment = false;

        for (var i = startDate.Day; i <= endDate.Day; ++i)
        {
            overlaps[i] = 0;

            foreach (var bill in billsWithRanges)
            {
                foreach (var range in bill.BillingRange)
                {
                    if (range.Start <= i && i <= range.End)
                        overlaps[i]++;
                }
            }

            if (!inFullPayment && overlaps[i] == filteredBills.Count()) { currentStart = i; inFullPayment = true; }
            if (inFullPayment && overlaps[i] != filteredBills.Count())
            {
                fullPaymentDates.Add(new(currentStart, i - 1)); inFullPayment = false;
            }
        }

        if (inFullPayment)
        {
            fullPaymentDates.Add(new(currentStart, endDate.Day));
        }

        var stats = new List<StatDTO>();

        if (fullPaymentDates.Count() > 0)
            stats.Add(new StatDTO {
                    Type = "FULL_PAYMENT_DATES",
                    BillIds = filteredBills.Select(b => b.Id),
                    DateRanges = fullPaymentDates
            });

        return new BillStatsDTO
        {
            StartDate = startDate,
            EndDate = endDate,
            Stats = stats
        };
    }
}
