using System.IO;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
// using Payobills.Bills.Services.Contracts;
using System.Text.Json.Serialization;

namespace Payobills.NocoDB;

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

    public async Task<NocoDBPage<T>?> GetRecordsPageAsync<T>(string baseName, string table, string fields = "*", string filter = "")
    {
        var filterParam = string.IsNullOrWhiteSpace(filter) ? string.Empty : $"&w={filter}";

        using var request = new HttpRequestMessage(
          HttpMethod.Get,
          $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}?fields={fields}{filterParam}"
        );

        request.Headers.Add("xc-token", nocoDBOptions.XCToken);

        var response = await httpClient.SendAsync(request);
        var responseStream = await response.Content.ReadAsStreamAsync();
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
        var recordsPage = await JsonSerializer.DeserializeAsync<NocoDBPage<T>>(responseStream, options);

        return recordsPage;
    }

    public async Task<T?> GetRecordByIdAsync<T>(string id, string baseName, string table, string fields)
    {
        using var request = new HttpRequestMessage(
          HttpMethod.Get,
          $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}/{id}?fields={fields}"
        );

        request.Headers.Add("xc-token", nocoDBOptions.XCToken);

        var response = await httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // Return default value - null for 404 response
            return default(T);
        }

        var responseStream = await response.Content.ReadAsStreamAsync();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
        var recordsPage = await JsonSerializer.DeserializeAsync<T>(responseStream, options);

        return recordsPage;
    }

    public async Task<IList<TOutput>> BulkCreateRecordsAsync<TInput, TOutput>(string baseName, string table, IList<TInput> payload)
    {
        using var jsonStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(jsonStream, payload);
        jsonStream.Seek(0, SeekOrigin.Begin);

        using var contentStream = new StreamContent(jsonStream);
        contentStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        using var request = new HttpRequestMessage(
          HttpMethod.Post,
          $"{nocoDBOptions.BaseUrl}/api/v1/db/data/bulk/nc/{baseName}/{table}"
        )
        {
            Content = contentStream
        };

        request.Headers.Add("xc-token", nocoDBOptions.XCToken);

        var response = await httpClient.SendAsync(request);
        var responseStream = await response.Content.ReadAsStreamAsync();
        var createdRecords = await JsonSerializer.DeserializeAsync<List<TOutput>>(responseStream);

        return createdRecords!;
    }

    public async Task<IList<TOutput>> BulkDeleteRecordsAsync<TInput, TOutput>(string baseName, string table, IList<TInput> payload)
    {
        using var jsonStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(jsonStream, payload);
        jsonStream.Seek(0, SeekOrigin.Begin);

        using var contentStream = new StreamContent(jsonStream);
        contentStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        using var request = new HttpRequestMessage(
          HttpMethod.Delete,
          $"{nocoDBOptions.BaseUrl}/api/v1/db/data/bulk/nc/{baseName}/{table}"
        )
        {
            Content = contentStream
        };

        request.Headers.Add("xc-token", nocoDBOptions.XCToken);

        var response = await httpClient.SendAsync(request);
        var responseStream = await response.Content.ReadAsStreamAsync();
        var deletedRecords = await JsonSerializer.DeserializeAsync<List<TOutput>>(responseStream);

        return deletedRecords!;
    }


    public async Task<TOutput> CreateRecordAsync<TInput, TOutput>(string baseName, string table, TInput payload)
    {
        using var jsonStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(jsonStream, payload);
        jsonStream.Seek(0, SeekOrigin.Begin);

        // Console.WriteLine("jsonStream length: " + jsonStream.Length);
        // Console.WriteLine("input payload");
        // Console.WriteLine(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));

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
        var responseString = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
        var createdRecord = JsonSerializer.Deserialize<TOutput>(responseString, options);

        // Console.WriteLine("response string");
        // Console.WriteLine(responseString);

        // Console.WriteLine("created record");
        // Console.WriteLine(JsonSerializer.Serialize(createdRecord, new JsonSerializerOptions { WriteIndented = true }));

        return createdRecord!;
    }

    public async Task<TOutput> UpdateRecordAsync<TInput, TOutput>(string baseName, string table, string id, TInput payload)
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
        var updatedRecord = await JsonSerializer.DeserializeAsync<TOutput>(responseStream);

        return updatedRecord!;
    }
}

