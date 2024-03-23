public record NocoDBPageInfo
{
   public long TotalRows;
   public double Page;
   public long PageSize;
   public bool IsFirstPage;
   public bool IsLastPage;
}