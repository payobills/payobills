using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Services;
using Payobills.Payments.NocoDB;
using AutoMapper;

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

// options
builder.Services.Configure<NocoDBOptions>(builder.Configuration.GetRequiredSection(nameof(NocoDBOptions)));

// bills
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<NocoDBClientService>();
builder.Services.AddScoped<ITransactionsService, TransactionsNocoDBService>();
// builder.Services.AddScoped<StatsQueryService>();

// utils
builder.Services.AddSingleton<IGuidService, GuidService>();
builder.Services.AddSingleton<IDateTimeService, DateTimeService>();

// mapper
builder.Services.AddSingleton<IMapper>((_) =>
{
  var config = new MapperConfiguration(cfg =>
  {
  });

  return new Mapper(config);
});

builder.Services
  .AddGraphQLServer()
  .AddSorting()
  .AddQueryType<Query>()
  .AddMutationType<Mutation>()
  .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
  .AddDiagnosticEventListener<ErrorLoggingDiagnosticsEventListener>();

var app = builder.Build();

app.MapGet("/", () => (new { app = "payobills.payments" }));
app.MapGraphQL();

app.Run();
