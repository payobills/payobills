using System.Text.Json.Serialization;

public record NocoDBPage<T>
{
    [JsonPropertyName("list")]
    public IEnumerable<T> List { get; set; } = Array.Empty<T>();
    [JsonPropertyName("pageInfo")]
    public NocoDBPageInfo PageInfo;
}