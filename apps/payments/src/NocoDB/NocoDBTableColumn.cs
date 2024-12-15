using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Payobills.Payments.NocoDB;


public record NocoDBTableColumn : NocoDBMetaResourceIdWithTitle
{
    [JsonPropertyName("colOptions")]
    public JsonNode ColOptions { get; set; } = null!;
}
