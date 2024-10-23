using System.Text.Json.Serialization;

public record NocoDBFile
{
    public string CorrelationID { get; set; } = string.Empty;

    public Dictionary<string,string> Tags { get; set; } = [];
    
    public IEnumerable<File> Files { get; set; } = Enumerable.Empty<File>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private DateTime updatedAt;

    [JsonPropertyName("updatedAt")]
    public string UpdatedAtString 
    {
        get => updatedAt.ToString("O");
        set
        {
            updatedAt = string.IsNullOrWhiteSpace(value) ? CreatedAt : DateTime.Parse(value);
            UpdatedAt = updatedAt;
        }
    }
}

public class File {
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("mimetype")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("signedPath")]
    public string SignedPath { get; set; } = string.Empty;
}