using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Doc: How to write all logs in JSON format
// https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter#json
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = false;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    options.JsonWriterOptions = new JsonWriterOptions
    {
        Indented = false
    };
});

var app = builder.Build();

app.MapGet("/", () => (new { app = "payobills.payments" }));

app.Run();
