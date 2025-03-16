using Microsoft.Extensions.Options;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using Payobills.BillsParser.Data.Contracts.Models;

/**
# Summary
- Read message from RabbitMQ
- Get Bill Record from NocoDB
- Download the Raw File to be parsed from the URL in the File Record
- Perform OCR on the file
- Store OCR data in NocoDB
- Parse text from OCR to billed transactions
- Store parsed data in transactions (ideally should be done in separate event cycle - Future scope)
- Mark the message as processed
*/

class RabbitMessage
{
    public required string Type { get; set; }
    public Dictionary<string, string> Args { get; set; } = new();
}


class Program
{
    static void Main()
    {
        ConnectionFactory factory = new ConnectionFactory();
        var queueConnectionStringUri = Environment.GetEnvironmentVariable("EVENT_QUEUE_CONNECTION_STRING") ?? throw new ArgumentNullException("EVENT_QUEUE_CONNECTION_STRING env is missing");
        factory.Uri = new Uri(queueConnectionStringUri);

        IConnection conn = factory.CreateConnection();
        IModel channel = conn.CreateModel();

        var nocoDbOptions = Options.Create<NocoDBOptions>(new NocoDBOptions
        {
            BaseUrl = Environment.GetEnvironmentVariable("NocoDBOptions__BaseUrl") ?? throw new ArgumentNullException("NotionDB BaseUrl env is missing"),
            XCToken = Environment.GetEnvironmentVariable("NocoDBOptions__XCToken") ?? throw new ArgumentNullException("NotionDB XCToken env is missing"),
        });

        var nocoDbBaseName = "payobills"; // Environment.GetEnvironmentVariable("NocoDBOptions__BaseName") ?? throw new ArgumentNullException("NotionDB BaseName env is missing");

        var nocodb = new NocoDBClientService(nocoDbOptions, new HttpClient());

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (_, eventArgs) =>
      {
          Console.WriteLine("=============== Started message consumption ===============");
          var body = eventArgs.Body.ToArray();
          string messageString = Encoding.UTF8.GetString(body);
          // {"type":"payobills.files.uploaded","args":{"correlationId":"cd654ad0-6484-459f-adfa-2a34b9baa9df"}}

          var message = JsonSerializer.Deserialize<RabbitMessage>(messageString, new JsonSerializerOptions
          {
              PropertyNameCaseInsensitive = true
          });

          if (message is null) return;

          Console.WriteLine($"Received: {messageString}");

          var fileRecord = await nocodb.GetRecordByIdAsync<NocoDBFile>(message.Args["id"], nocoDbBaseName, "files", "*")
              ?? throw new Exception("NocoDBFile not found");

          var fileUrl = fileRecord.Files.ElementAt(0).SignedPath;
          using var httpClient = new HttpClient();
          using (var stream = await httpClient.GetStreamAsync($"{nocoDbOptions.Value.BaseUrl}/{fileUrl}"))
          {
              var filePath = Path.Combine("/tmp", "test.pdf");
              var memoryStream = new MemoryStream();

              await stream.CopyToAsync(memoryStream);
              await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
          }

          using var document = PdfDocument.Open("/tmp/test.pdf", new ParsingOptions() { ClipPaths = true });
          var statementStringBuilder = new StringBuilder();

          var transactions = new List<TransactionInput>();

          // Console.WriteLine($"\"LineNumber\",\"ParsedDate\",\"Merchant\",\"Amount\",\"Type\"");
          foreach (var pageNumber in Enumerable.Range(1, document.NumberOfPages))
          {
              var stream = new Camelot.Parsers.Stream();
              var tables = stream.ExtractTables(document.GetPage(pageNumber));

              foreach (var table in tables)
              {
                  for (int i = 0; i < table.Rows.Count; i++)
                  {
                      for (int j = 0; j < table.Cols.Count; j++)
                      {
                          var cellText = table.Cells[i][j].Text.Trim(new char[] { '\n', '\r' });
                          cellText = Regex.Replace(cellText, @"[/\r\n/]", " ");
                          statementStringBuilder.Append($" {cellText} ");
                      }
                      statementStringBuilder.AppendLine();
                  }
              }
          }

          var ocrRecord = new OCRFile
          {
              ExtractedRawText = statementStringBuilder.ToString(),

              File = new NocoDbFileInput
              {
                  Id = message.Args["id"]
              }
          };

          var ocrId = 0;
          if (fileRecord.OCR is null)
          {
              var createdOcrRecord = await nocodb.CreateRecordAsync<OCRFile, OCRFileOutput>(nocoDbBaseName, "ocr", ocrRecord);
              ocrId = createdOcrRecord.Id;
              Console.WriteLine($"Writing parsed transactions - {transactions.Count}");
          }
          else
          {
              ocrId = fileRecord.OCR.Id;
              var filter = $"w=(OcrId,eq,{fileRecord.OCR.Id.ToString()})&l=1000";
              await nocodb.UpdateRecordAsync<OCRFile, OCRFileOutput>(nocoDbBaseName, "ocr", fileRecord.OCR.Id.ToString(), ocrRecord);
          }

          Console.WriteLine("=============== Finished message consumption ===============");
          channel.BasicAck(eventArgs.DeliveryTag, true);
      };

        string consumerTag = channel.BasicConsume("payobills.files", false, consumer);

        Console.WriteLine("Waiting on messages...");

        var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationTokenSource.Cancel();
            };

        try
        {
            cancellationTokenSource.Token.WaitHandle.WaitOne();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Shutting down gracefully...");
        }
    }
}
/**
{
    "ExtractedRawText": "Test Bill 2 - Postman",
    "File": {
        "Id": 8
    }
}
 */

public record NocoDbBulkRecord
{
    public int Id { get; set; }
}

public record NocoDbFileInput
{
    public required string Id { get; set; }
}
public record OCRFile
{
    public string ExtractedRawText { get; set; } = string.Empty;
    public required NocoDbFileInput File { get; set; }
}

public record NocoDbFileOutput
{
    public int Id { get; set; }
}
public record OCRFileOutput
{
    public required int Id { get; set; }
    public string ExtractedRawText { get; set; } = string.Empty;
    public required NocoDbFileOutput File { get; set; }
}

