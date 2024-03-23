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
}
