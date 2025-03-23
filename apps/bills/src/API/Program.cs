using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services;
using Payobills.NocoDB;
using AutoMapper;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

// Doc: How to write all logs in JSON format
// https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter#json
// builder.Logging.AddJsonConsole(options =>
// {
//     options.IncludeScopes = false;
//     options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
//     options.JsonWriterOptions = new JsonWriterOptions
//     {
//         Indented = false
//     };
// });
builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;

    // Send logs directly to the OpenTelemetry sidecar
    options.AddOtlpExporter(otlpOptions =>
    {
        otlpOptions.Endpoint = new Uri("http://localhost:4317"); // gRPC
    });
});

// options
builder.Services.Configure<NocoDBOptions>(builder.Configuration.GetRequiredSection(nameof(NocoDBOptions)));

// bills
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<NocoDBClientService>();
builder.Services.AddScoped<IBillsService, BillsNocoDBService>();
builder.Services.AddScoped<IBillStatementsService, BillStatementsNocoDBService>();
builder.Services.AddScoped<StatsQueryService>();

// utils
builder.Services.AddSingleton<IGuidService, GuidService>();
builder.Services.AddSingleton<IDateTimeService, DateTimeService>();

// mapper
builder.Services.AddSingleton<IMapper>((_) =>
{
  var config = new MapperConfiguration(cfg =>
  {
    cfg.AddProfile<BillsMappingProfile>();
  });

  return new Mapper(config);
});

// gql
builder.Services
  .AddGraphQLServer()
  .AddQueryType<Query>()
  .AddMutationType<Mutation>()
  .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
  .AddDiagnosticEventListener<ErrorLoggingDiagnosticsEventListener>();

var app = builder.Build();

app.MapGet("/", () => (new { app = "payobills.bills" }));
app.MapGraphQL();

app.Run();
