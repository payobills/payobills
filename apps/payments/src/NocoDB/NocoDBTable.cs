using System.Text.Json.Serialization;

namespace Payobills.Payments.NocoDB;

public record NocoDBTable
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("title")]

    public required string Title { get; set; }
    [JsonPropertyName("columns")]

    public IEnumerable<NocoDBTableColumn> Columns { get; set; } = [];
}
