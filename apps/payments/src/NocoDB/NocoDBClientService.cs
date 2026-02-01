using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payobills.Payments.Services.Contracts;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Linq;
using Payobills.Payments.Services.Contracts.DTOs;
using System.Data;

namespace Payobills.Payments.NocoDB;

public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
{
  public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return DateTime.Parse(reader.GetString() ?? string.Empty);
  }

  public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
  {
    writer.WriteStringValue(value.ToString());
  }
}

public class NocoDBClientService
{
  private readonly HttpClient httpClient;
  private readonly NocoDBOptions nocoDBOptions;

  private readonly JsonSerializerOptions jsonSerializerOptions;

  public NocoDBClientService(
    IOptions<NocoDBOptions> nocoDBOptions,
    HttpClient httpClient
  )
  {
    this.httpClient = httpClient;
    this.nocoDBOptions = nocoDBOptions.Value;

    jsonSerializerOptions = new JsonSerializerOptions();
    jsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    // jsonSerializerOptions.Converters.Add(new JsonNodeParse());
  }

  public async Task<NocoDBPage<T>?> GetRecordsPageAsync<T>(string baseName, string table, string fields, string extraArgs = "")
  {
    var extraArgsToPassInUrl = !String.IsNullOrEmpty(extraArgs) ? $"&{extraArgs}" : String.Empty;
    var url = $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}?fields={fields}&{extraArgsToPassInUrl}";

    Console.WriteLine($"Fetching transactions from NocoDB: {url}");
    // Console.WriteLine($"Fetching transactions from NocoDB: {extraArgs}");

    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      url
    );

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);

    var response = await httpClient.SendAsync(request);
    var responseStream = await response.Content.ReadAsStreamAsync();
    var recordsPage = await JsonSerializer.DeserializeAsync<NocoDBPage<T>>(responseStream, jsonSerializerOptions);

    return recordsPage;
  }

  public async Task<T?> GetRecordByIdAsync<T>(string id, string baseName, string table, string fields, string extraArgs = "")
  {
    var extraArgsToPassInUrl = !String.IsNullOrEmpty(extraArgs) ? $"&{extraArgs}" : String.Empty;
    var url = $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}/{id}?fields={fields}{extraArgsToPassInUrl}";
    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      url
    );

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);
    var response = await httpClient.SendAsync(request);

    if (response.StatusCode == HttpStatusCode.NotFound)
    {
      // Return default value - null for 404 response
      return default;
    }

    var responseStream = await response.Content.ReadAsStreamAsync();
    var recordsPage = await JsonSerializer.DeserializeAsync<T>(responseStream, jsonSerializerOptions);

    return recordsPage;
  }

  public async Task<string?> GetRecordByIdAsync(string id, string baseName, string table, string fields, string extraArgs = "")
  {
    var extraArgsToPassInUrl = !String.IsNullOrEmpty(extraArgs) ? $"&{extraArgs}" : String.Empty;
    var url = $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}/{id}?fields={fields}{extraArgsToPassInUrl}";
    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      url
    );

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);

    var response = await httpClient.SendAsync(request);

    if (response.StatusCode == HttpStatusCode.NotFound)
    {
      // Return default value - null for 404 response
      return default;
    }

    var responseJsonString = await response.Content.ReadAsStringAsync();
    return responseJsonString;
  }

  public async Task<TOutput> CreateRecordAsync<TInput, TOutput>(string baseName, string table, TInput payload)
  {
    using var jsonStream = new MemoryStream();
    await JsonSerializer.SerializeAsync(jsonStream, payload);
    jsonStream.Seek(0, SeekOrigin.Begin);

    using var contentStream = new StreamContent(jsonStream);
    contentStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

    using var request = new HttpRequestMessage(
      HttpMethod.Post,
      $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}?fields=*"
    )
    {
      Content = contentStream
    };

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);

    var response = await httpClient.SendAsync(request);
    var responseStream = await response.Content.ReadAsStreamAsync();
    var createdRecord = await JsonSerializer.DeserializeAsync<TOutput>(responseStream, this.jsonSerializerOptions);

    return createdRecord!;
  }

  public async Task<TOutput> UpdateRecordAsync<TInput, TOutput>(string id, string baseName, string table, TInput payload, JsonSerializerOptions? jsonSerializerOptions = default)
  {
    using var jsonStream = new MemoryStream();
    await JsonSerializer.SerializeAsync(jsonStream, payload);
    jsonStream.Seek(0, SeekOrigin.Begin);

    using var contentStream = new StreamContent(jsonStream);
    contentStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

    using var request = new HttpRequestMessage(
      HttpMethod.Patch,
      $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}/{id}"
    )
    {
      Content = contentStream
    };

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);

    var response = await httpClient.SendAsync(request);
    var responseStream = await response.Content.ReadAsStreamAsync();

    var updatedRecord = await JsonSerializer.DeserializeAsync<TOutput>(responseStream, this.jsonSerializerOptions);

    return updatedRecord!;
  }

  public async Task<TOutput> GetMetaResourceDataAsync<TOutput>(string metaUrl)
  {
    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      $"{nocoDBOptions.BaseUrl}/{metaUrl}"
    );

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);
    var response = await httpClient.SendAsync(request);

    // Note: Kept for easy logging when debugging
    // var responseText = await response.Content.ReadAsStringAsync();
    // Console.WriteLine(responseText);
    // JsonSerializerOptions options = new JsonSerializerOptions();
    // options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    // var metaResourceData = JsonSerializer.Deserialize<TOutput>(responseText, options);
    // return metaResourceData!;

    var responseStream = await response.Content.ReadAsStreamAsync();

    var metaResourceData = await JsonSerializer.DeserializeAsync<TOutput>(responseStream, jsonSerializerOptions);
    return metaResourceData!;
  }

  public async Task<T?> ParseJsonToNocoDBRecordAsync<T>(string jsonString)
  {
    // Console.WriteLine(jsonString);
    if (string.IsNullOrEmpty(jsonString))
    {
      return default;
    }

    using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString));
    jsonStream.Seek(0, SeekOrigin.Begin);

    var record = await JsonSerializer.DeserializeAsync<T>(jsonStream, jsonSerializerOptions);
    return record;
  }

  public async Task<NocoDBPage<T>> GetManyToManyLinkRecordsAsync<T>(
   string baseName,
   string table,
   string recordId,
   string relationLinkName)
  {
    var url = $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}/{recordId}/mm/{relationLinkName}";

    var request = new HttpRequestMessage(
      HttpMethod.Get,
      url
    );

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);
    var response = await httpClient.SendAsync(request);

    var responseStream = await response.Content.ReadAsStreamAsync();

    var recordsPage = await JsonSerializer.DeserializeAsync<NocoDBPage<T>>(responseStream, jsonSerializerOptions);

    return recordsPage;
  }

  public async Task LinkManyToManyRecordsAsync(
    string baseName,
    string table,
    string recordId,
    string relationLinkName,
    IEnumerable<string> relatedRecordIds,
    NocoDbLinkOperation operation = NocoDbLinkOperation.Attach)
  {
    if (!relatedRecordIds.Any()) return;

    var url = $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}/{recordId}/mm/{relationLinkName}";

    var linkTasks = relatedRecordIds.Select(id =>
    {
      return new HttpRequestMessage(
        operation is NocoDbLinkOperation.Attach ? HttpMethod.Post : HttpMethod.Delete,
        $"{url}/{id}"
        );
    });

    await Task.WhenAll(linkTasks.Select(async request =>
    {
      request.Headers.Add("xc-token", nocoDBOptions.XCToken);
      var response = await httpClient.SendAsync(request);
      try { response.EnsureSuccessStatusCode(); }
      catch (Exception)
      {
        Console.Error.WriteLine($"--- \n Error linking receipt: {request.RequestUri} \n {await response.Content.ReadAsStringAsync()}");
      }
    }));
  }

  static JsonNode? ConvertJsonElementToJsonNode(JsonElement element)
  {
    return element.ValueKind switch
    {
      JsonValueKind.Null => null,
      JsonValueKind.Array => JsonArray.Create(element),
      JsonValueKind.Object => JsonObject.Create(element),
      _ => JsonValue.Create(element)
    };
  }

  private (NocoDBTableColumn column, bool changedRequired) addMultiSelectOptionToColumn(NocoDBTableColumn column, string? optionId, string newOptionName)
  {
    var changeRequired = false;

    var options = new JsonArray();
    var colOptions = new JsonObject
    {
        { "options", options }
    };
    var output = new JsonObject();
    var propertiesToSelect = new List<string> { "title", "column_name", "uidt" };

    var json = JsonSerializer.Deserialize<JsonDocument>(column.ColOptions.ToString());
    foreach (var p in from p in json.RootElement.EnumerateObject()
                      where propertiesToSelect.Any(x => x == p.Name)
                      select p)
    {
      output.Add(p.Name, ConvertJsonElementToJsonNode(p.Value));
    }

    foreach (var property in json.RootElement.GetProperty("options").EnumerateArray())
    {
      options.Add(property);
    }

    

    if (!string.IsNullOrEmpty(optionId))
    {
      foreach (var option in options)
      {
        if (option["id"]?.GetValue<string>() == optionId)
        {
          if(option["title"]?.GetValue<string>() != newOptionName) 
          {
            changeRequired = true;
          }

          option["title"] = newOptionName;
        }
      }
    }
    else
    {
      changeRequired = true;
      var newOption = new JsonObject
      {
        ["title"] = newOptionName
      };
      var newOptionJsonString = newOption.ToJsonString();
      options.Add(JsonDocument.Parse(newOptionJsonString).RootElement);
    }

    return (new NocoDBTableColumn
    {
      Id = column.Id,
      Title = column.Title,
      UIDataType = column.UIDataType,
      ColOptions = colOptions
    }, changeRequired);
  }

  public async Task<(string Id, string OptionName)> UpdateMultiSelectColumnMetadata(
  NocoDBTableColumn column, string? optionId, string newOptionName)
  {

    if (column.UIDataType != "MultiSelect")
      throw new InvalidOperationException("Column is not of MultiSelect type.");

    // Throw error if Tags column has the same dto.Title option already
    var existingTagOption = column.ColOptions.Deserialize<NocoDBColOptions>()?.Options.FirstOrDefault(p => p.Title == newOptionName);
    if (optionId == null && existingTagOption != null)
    {
      throw new DuplicateNameException($"Tag with title '{newOptionName}' already exists.");
    }

    var url = $"{nocoDBOptions.BaseUrl}/api/v1/db/meta/columns/{column.Id}";

    var (columnToUpdate, changeRequired) = addMultiSelectOptionToColumn(column, optionId, newOptionName);

    if (!changeRequired)
    {
      return (optionId!, newOptionName);
    }

    using var jsonStream = new MemoryStream();
    await JsonSerializer.SerializeAsync(jsonStream, columnToUpdate);
    jsonStream.Seek(0, SeekOrigin.Begin);

    using var contentStream = new StreamContent(jsonStream);
    contentStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

    Console.WriteLine($"[HTTP] {HttpMethod.Patch} {url}");
    using var request = new HttpRequestMessage(
      HttpMethod.Patch,
      url
    )
    { Content = contentStream };

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);

    var response = await httpClient.SendAsync(request);

    if (response.StatusCode != HttpStatusCode.OK)
    {
      var responseText = await response.Content.ReadAsStringAsync();
      throw new InvalidOperationException($"Failed to update multi-select column metadata. Status: {response.StatusCode}, Response: {responseText}");
    }

    var responseStream = await response.Content.ReadAsStreamAsync();
    var updatedTable = await JsonSerializer.DeserializeAsync<NocoDBTable>(responseStream, jsonSerializerOptions);
    var updatedColumn = updatedTable?.Columns.FirstOrDefault(p => p.Title == column.Title) ?? throw new KeyNotFoundException("Could not find updated column");
    var colOptions = updatedColumn.ColOptions["options"]!.AsArray();
    var id = colOptions.FirstOrDefault(o => o?["title"]?.ToString() == newOptionName)?["id"]?.GetValue<string>() ?? throw new KeyNotFoundException("Could not find newly added option ID");

    return (id, newOptionName);
  }
}
