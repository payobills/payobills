using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Microsoft.AspNetCore.Mvc;

/**
# Summary
- Get fileId from the request
- Get Bill Record from NocoDB
- Download the Raw File to be parsed from the URL in the File Record
- Perform OCR on the file
- Return OCR text as response
*/

class RabbitMessage
{
    public required string Type { get; set; }
    public Dictionary<string, string> Args { get; set; } = new();
}

partial class Program
{
    static async Task<object> GetOcrStringAsync(string fileId)
    {
        var nocoDbOptions = Options.Create<NocoDBOptions>(new NocoDBOptions
        {
            BaseUrl = Environment.GetEnvironmentVariable("NocoDBOptions__BaseUrl") ?? throw new ArgumentNullException("NotionDB BaseUrl env is missing"),
            XCToken = Environment.GetEnvironmentVariable("NocoDBOptions__XCToken") ?? throw new ArgumentNullException("NotionDB XCToken env is missing"),
        });

        var nocoDbBaseName = "payobills"; // Environment.GetEnvironmentVariable("NocoDBOptions__BaseName") ?? throw new ArgumentNullException("NotionDB BaseName env is missing");
        var nocodb = new NocoDBClientService(nocoDbOptions, new HttpClient());

        Console.WriteLine($"=============== Starting ocr extraction (fileId = {fileId})===============");

        var fileRecord = await nocodb.GetRecordByIdAsync<NocoDBFile>(fileId, nocoDbBaseName, "files", "*")
            ?? throw new Exception("NocoDBFile not found");

        var fileUrl = fileRecord.Files.ElementAt(0).SignedPath;
        using var httpClient = new HttpClient();
        using (var stream = await httpClient.GetStreamAsync($"{nocoDbOptions.Value.BaseUrl}/{fileUrl}"))
        {
            var filePath = Path.Combine("/tmp", $"file-${fileId}.pdf");
            var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream);
            await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
        }

        using var document = PdfDocument.Open($"/tmp/file-${fileId}.pdf", new ParsingOptions() { ClipPaths = true });
        var fileStringBuilder = new StringBuilder();

        foreach (var pageNumber in Enumerable.Range(1, document.NumberOfPages))
        {
            var stream = new Camelot.Parsers.Stream();
            List<Camelot.Core.Table> tables;

            try
            {
                Page page = document.GetPage(pageNumber);
                tables = stream.ExtractTables(page);
                foreach (var table in tables)
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        for (int j = 0; j < table.Cols.Count; j++)
                        {
                            var cellText = table.Cells[i][j].Text.Trim(['\n', '\r']);
                            cellText = MyRegex().Replace(cellText, " ");
                            fileStringBuilder.Append($" {cellText} ");
                        }
                        fileStringBuilder.AppendLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error: {e}");
                continue;
            }
        }

        Console.WriteLine($"=============== Finished ocr extraction (fileId = {fileId})===============");
        return new { OcrText = fileStringBuilder.ToString() };
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/ocr", ([FromQuery(Name = "fileId")] string fileId) => GetOcrStringAsync(fileId));
        app.Run();
    }

    [GeneratedRegex(@"[/\r\n/]")]
    private static partial Regex MyRegex();
}

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
    // public required NocoDbFileOutput File { get; set; }
}
