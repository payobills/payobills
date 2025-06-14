using System.IO;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payobills.Payments.Services.Contracts;
using System.Text.Json.Serialization;

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

  public NocoDBClientService(
    IOptions<NocoDBOptions> nocoDBOptions,
    HttpClient httpClient
  )
  {
    this.httpClient = httpClient;
    this.nocoDBOptions = nocoDBOptions.Value;
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
    JsonSerializerOptions options = new JsonSerializerOptions();
    options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    var recordsPage = await JsonSerializer.DeserializeAsync<NocoDBPage<T>>(responseStream, options);

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
    JsonSerializerOptions options = new JsonSerializerOptions();
    options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    var recordsPage = await JsonSerializer.DeserializeAsync<T>(responseStream, options);

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

    JsonSerializerOptions options = new();
    options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    var createdRecord = await JsonSerializer.DeserializeAsync<TOutput>(responseStream, options);

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

    JsonSerializerOptions options = jsonSerializerOptions ?? new();
    options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    var updatedRecord = await JsonSerializer.DeserializeAsync<TOutput>(responseStream, options);

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
    JsonSerializerOptions options = new JsonSerializerOptions();
    options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    var metaResourceData = await JsonSerializer.DeserializeAsync<TOutput>(responseStream, options);
    return metaResourceData!;
  }

  public async Task<T?> ParseJsonToNocoDBRecordAsync<T>(string jsonString)
  {
    if (string.IsNullOrEmpty(jsonString))
    {
      return default;
    }

    using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString));
    jsonStream.Seek(0, SeekOrigin.Begin);

    JsonSerializerOptions options = new JsonSerializerOptions();
    options.Converters.Add(new DateTimeConverterUsingDateTimeParse());

    var record = await JsonSerializer.DeserializeAsync<T>(jsonStream, options);
    return record;
  }

  public async  Task<NocoDBPage<T>> GetManyToManyLinkRecordsAsync<T>(
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
    JsonSerializerOptions options = new JsonSerializerOptions();
    options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    var recordsPage = await JsonSerializer.DeserializeAsync<NocoDBPage<T>>(responseStream, options);

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
}
