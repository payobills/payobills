using Payobills.Bills.Services.Contracts;
using Payobills.Bills.Services;
using Payobills.Bills.Data;
using Payobills.Bills.Data.Contracts;
using MongoDB.Driver;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

var CORS_POLICY_NAME = "allowedOrigins";
builder.Services.AddCors(options =>
{
  var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
  if (!string.IsNullOrEmpty(allowedOrigins))
  {
    options.AddPolicy(name: CORS_POLICY_NAME, policy
        => policy.WithOrigins(allowedOrigins.Split(",")));
  }
});

// bills
builder.Services.AddScoped<IBillsService, BillsService>();
builder.Services.AddScoped<IBillsRepo, BillsRepo>();
builder.Services.AddScoped<IBillsContext, BillsContext>();
builder.Services.AddScoped<IMongoClient>((_) =>
{
  var connectionString = Environment.GetEnvironmentVariable("BILLS_DB_CONNECTION_STRING");
  if (string.IsNullOrEmpty(connectionString))
    throw new ArgumentNullException($"{connectionString}, the connection string cannot be null.");
  var mongoClient = new MongoClient(connectionString);
  return mongoClient;
});

// utils
builder.Services.AddSingleton<IGuidService, GuidService>();
builder.Services.AddSingleton<IDateTimeService, DateTimeService>();

// mapper
builder.Services.AddSingleton<IMapper>((_) => {
  var config = new MapperConfiguration(cfg => {
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

app.UseCors(CORS_POLICY_NAME);

app.MapGet("/", () => (new { app = "payobills.bills" }));
app.MapGraphQL();

app.Run();
