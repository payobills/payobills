using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Payobills.Payments.NocoDB;


public record NocoDBTableColumn : NocoDBMetaResourceIdWithTitle
{
    [JsonPropertyName("column_name")]
    public string ColumnName => Title;

    [JsonPropertyName("uidt")]
    public string UIDataType { get; set; } = string.Empty;

    [JsonPropertyName("colOptions")]
    public JsonNode ColOptions { get; set; } = null!;
}
