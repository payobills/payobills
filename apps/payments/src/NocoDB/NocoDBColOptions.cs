using System.Text.Json.Serialization;

namespace Payobills.Payments.NocoDB;

public record NocoDBColOptions
{
    [JsonPropertyName("options")]
    public required IEnumerable<NocoDBMetaResourceIdWithTitle> Options { get; set; }
     = [];
}
