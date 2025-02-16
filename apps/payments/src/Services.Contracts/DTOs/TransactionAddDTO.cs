using System.Text.Json.Serialization;
using HotChocolate;
using Payobills.Payments.Data.Contracts.Models;

namespace Payobills.Payments.Services.Contracts.DTOs
{
    public class TransactionAddDTO
    {
      public string TransactionText { get; set; } = string.Empty;
      public string ParseStatus { get; set; } = "NotStarted";

      // Navigation property: Represents which bill this transaction belongs to.
      [JsonPropertyName("bills")]
      public IdDTO<long> Bill { get; set; } = default!;

      [GraphQLIgnore]
      public string BackDateString { get; set; }

      [GraphQLIgnore]
      public string SourceSystemID { get; set; }

      [GraphQLIgnore]
      public string SourceType { get; set; } = "PAYOBILLS_APP";
      public string Merchant { get; set; } = string.Empty;
      public double? Amount { get; set; }
        // public string? Currency { get; set; }
      public string? Notes { get { return notes; } set { notes = value ?? string.Empty; } }
      private string notes = string.Empty;
        // public string? BackDateString { get; set; } = string.Empty;
        // public DateTime? BackDate { get; set; }
        //
        // [GraphQLName("tags")]
        // [JsonIgnore]
        // public string[]? TagsArray { get; set; } = [];
        //
        // [GraphQLIgnore]
        // public string Tags => string.Join(",", TagsArray ?? []);
    }
}
