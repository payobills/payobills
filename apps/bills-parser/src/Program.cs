using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

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

ConnectionFactory factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://user:VbWRo76sbPvC43Su@localhost");

IConnection conn = factory.CreateConnection();
IModel channel = conn.CreateModel();

var nocoDbOptions = Options.Create<NocoDBOptions>(new NocoDBOptions
{
    BaseUrl = Environment.GetEnvironmentVariable("NocoDBOptions__BaseUrl") ?? throw new ArgumentNullException("NotionDB BaseUrl env is missing"),
    XCToken = Environment.GetEnvironmentVariable("NocoDBOptions__XCToken") ?? throw new ArgumentNullException("NotionDB XCToken env is missing"),
});

var nocodb = new NocoDBClientService(nocoDbOptions, new HttpClient());

var consumer = new EventingBasicConsumer(channel);
consumer.Received += async (_, eventArgs) =>
{
    Console.WriteLine("=============== Started message consumption ===============");
    var body = eventArgs.Body.ToArray();
    string message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Received: {message}");

    var fileRecord = await nocodb.GetRecordByIdAsync<NocoDBFile>("1", "p_uye5el98noe784", "muc28giyeqf2tqf", "*")
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
    var outputFilePath = "extracted-data.txt";
    System.IO.File.WriteAllText(outputFilePath, string.Empty);

    Console.WriteLine($"\"LineNumber\",\"ParsedDate\",\"Merchant\",\"Amount\",\"Type\"");
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

            System.IO.File.AppendAllText(outputFilePath, statementStringBuilder.ToString());
        }

        var statementString = statementStringBuilder.ToString();
        var lineNumber = 0;

        foreach (var line in statementString.Split(Environment.NewLine))
        {
            ++lineNumber;
            var currentLine = line.Trim();

            var columns = currentLine.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (columns.Length < 2) continue;
            bool allDetailsExtracted = true;

            bool parseMore = false;
            // Try to parse the first column as a date in "Month Date" format
            DateTime parsedDate = DateTime.MinValue;
            if (columns.Length > 0)
            {
                // Console.WriteLine($"trying to parse date - {columns[0]}");
                if (DateTime.TryParseExact($"{columns[0].Trim()} {columns[1].Trim()}", "MMMM dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    // Successfully parsed the date
                    parseMore = true;
                }
                else
                {
                    allDetailsExtracted = false;
                }
            }
            else
            {
                allDetailsExtracted = false;
            }

            if (!parseMore) continue;

            float amount = 0;
            if (columns.Length > 3)
            {
                if (!float.TryParse(columns.LastOrDefault() ?? "", out amount))
                {
                    allDetailsExtracted = false;
                }
            }
            else
            {
                allDetailsExtracted = false;
            }

            // Second column as a string (if available)
            var merchant = string.Join(" ", columns.Skip(2).SkipLast(2));

            var type = merchant.Contains("payment received", StringComparison.CurrentCultureIgnoreCase) ? "CREDIT" : "DEBIT";
            if (allDetailsExtracted)
            {
                Console.WriteLine($"\"{lineNumber}\",\"{parsedDate}\",\"{merchant.Trim()}\",\"{amount}\",\"{type}\"");
            }
        }
    }

    Console.WriteLine("=============== Finished message consumption ===============");
    channel.BasicAck(eventArgs.DeliveryTag, true);
};

string consumerTag = channel.BasicConsume("payobills.files", false, consumer);

Console.WriteLine("Waiting on messages...");
Console.ReadLine();
