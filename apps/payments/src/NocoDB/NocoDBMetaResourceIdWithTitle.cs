using System.Text.Json.Serialization;

namespace Payobills.Payments.NocoDB;

public record NocoDBMetaResourceIdWithTitle
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("title")]
    public required string Title { get; set; }
}
