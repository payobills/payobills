using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services;
using Payobills.Bills.NocoDB;
using AutoMapper;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = false;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    options.JsonWriterOptions = new JsonWriterOptions
    {
        Indented = false
    };
});

// options
builder.Services.Configure<NocoDBOptions>(builder.Configuration.GetRequiredSection(nameof(NocoDBOptions)));

// bills
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<NocoDBClientService>();
builder.Services.AddScoped<IBillsService, BillsNocoDBService>();

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
  .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

app.MapGet("/", () => (new { app = "payobills.bills" }));
app.MapGraphQL();

app.Run();
