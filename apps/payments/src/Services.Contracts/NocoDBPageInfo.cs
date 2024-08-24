namespace Payobills.Payments.Services.Contracts;

public record NocoDBPageInfo
{
   public long TotalRows { get; set; }
   public double Page { get; set; }
   public long PageSize { get; set; }
   public bool IsFirstPage { get; set; }
   public bool IsLastPage { get; set; }
}
