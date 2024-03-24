using System.IO;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Payobills.Bills.NocoBD;

public class NocoDBClientService
{
  private readonly HttpClient httpClient;
  private readonly NocoDBOptions nocoDBOptions;

  public NocoDBClientService(
    HttpClient httpClient,
    IOptions<NocoDBOptions> nocoDBOptions
  )
  {
    this.httpClient = httpClient;
    this.nocoDBOptions = nocoDBOptions.Value;
  }

  public async Task<NocoDBPage<T>?> GetRecordsPageAsync<T>(string baseName, string table, string fields)
  {
    using var request = new HttpRequestMessage(
      HttpMethod.Get,
      $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}?fields={fields}"
    );

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);

    var response = await httpClient.SendAsync(request);
    var responseStream = await response.Content.ReadAsStreamAsync();
    var recordsPage = await JsonSerializer.DeserializeAsync<NocoDBPage<T>>(responseStream);

    return recordsPage;
  }

  public async Task<T> GetRecordByIdAsync<T>(string id, string baseName, string table, string fields)
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
    var recordsPage = await JsonSerializer.DeserializeAsync<T>(responseStream);

    return recordsPage;
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
      $"{nocoDBOptions.BaseUrl}/api/v1/db/data/v1/{baseName}/{table}"
    )
    {
      Content = contentStream
    };

    request.Headers.Add("xc-token", nocoDBOptions.XCToken);

    var response = await httpClient.SendAsync(request);

    if (response.StatusCode == HttpStatusCode.NotFound)
    {
      // Return default value - null for 404 response
      return default(TOutput);
    }

    var responseStream = await response.Content.ReadAsStreamAsync();
    var recordsPage = await JsonSerializer.DeserializeAsync<TOutput>(responseStream);

    return recordsPage;
  }
}
