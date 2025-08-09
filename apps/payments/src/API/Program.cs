using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Services;
using Payobills.Payments.NocoDB;
using AutoMapper;
using RabbitMQ.Client;
using Payobills.Payments.RabbitMQ;

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
builder.Services.Configure<RabbitMQOptions>(builder.Configuration.GetRequiredSection(nameof(RabbitMQOptions)));

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

builder.Services.AddSingleton<RabbitMQService>();

// builder.Services.AddSingleton<>(async (_) =>
// {
//   var factory = new ConnectionFactory
//   {
//     Uri = new Uri(builder.Configuration.GetValue<string>("EVENT_QUEUE_CONNECTION_STRING") ?? string.Empty)
//   };

//   var connection = await factory.CreateConnectionAsync();
//   return await connection.CreateChannelAsync();
// });

builder.Services
  .AddGraphQLServer()
  .DisableIntrospection(false)
  .AddApolloFederation()
  .AddSorting()
  .AddQueryType<Query>()
  .AddMutationType<Mutation>()
  .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
  .AddDiagnosticEventListener<ErrorLoggingDiagnosticsEventListener>()
  .AddType<Payobills.Payments.Services.Contracts.DTOs.File>()
  .AddType<TransactionDTOType>();

var app = builder.Build();

app.MapGet("/", () => (new { app = "payobills.payments" }));
app.MapGraphQL();

await app.RunWithGraphQLCommandsAsync(args);
