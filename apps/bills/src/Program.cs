using payobills.bills.svc;
using payobills.bills.repos;
using payobills.bills.data;
using payobills.bills.gql;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

var serviceVersion = Environment.GetEnvironmentVariable("VERSION") ?? throw new ArgumentNullException("'VERSION' Environment Variable is missing.");

// https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore/README.md
var isTelemetryEnabled = (Environment.GetEnvironmentVariable("TELEMETRY_DISABLED") ?? "true") == "true";
if (isTelemetryEnabled)
{
  builder.Services
    .AddOpenTelemetry()
    // .WithMetrics(builder => builder.AddPrometheusExporter())
    .WithTracing(builder =>
    {
      builder
      .AddSource("ServiceA")
      .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ServiceA"))
      .AddAspNetCoreInstrumentation();
      // .AddConsoleExporter(options => {options.})
      // https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol/README.md
      // .AddOtlpExporter(options => {
      //   options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_ENDPOINT") ?? throw new ArgumentNullException("'OTEL_ENDPOINT' Environment Variable is missing."));
      // });
    });
}


var corsPolicyName = "allowedOrigins";
builder.Services.AddCors(options =>
{
  var allowedOrigins = Environment.GetEnvironmentVariable("BILLS_ALLOWED_ORIGINS");
  if (!String.IsNullOrEmpty(allowedOrigins))
  {
    options.AddPolicy(name: corsPolicyName, policy
        => policy.WithOrigins(allowedOrigins.Split(",")));
  }
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBillsService, BillsService>();
builder.Services.AddScoped<BillsRepo>();
builder.Services.AddDbContext<BillsContext>(options =>
{
  options.UseSqlite($"Data Source={Environment.GetEnvironmentVariable("BILLS_DB_PATH")}");
});
builder.Services.AddSingleton<IGuidService, GuidService>();
builder.Services.AddSingleton<IDateTimeService, DateTimeService>();
builder.Services
  .AddGraphQLServer()
  .AddQueryType<Query>()
  .AddMutationType<Mutation>()
  .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
  using (var billsContext = serviceScope.ServiceProvider.GetService<BillsContext>())
  {
    billsContext?.Database.EnsureCreated();
    billsContext?.SaveChanges();
  }
}

app.UseCors(corsPolicyName);
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//   app.UseSwagger();
//   app.UseSwaggerUI();
// }

app.UseAuthorization();
// app.UseOpenTelemetryPrometheusScrapingEndpoint();

// app.MapControllers();

app.MapGet("/", () => (new { app = "payobills.bills" }));

app.MapGraphQL();

app.Run();
