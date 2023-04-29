using payobills.bills.svc;
using payobills.bills.repos;
using payobills.bills.data;
using Microsoft.EntityFrameworkCore;
using payobills.bills.gql;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddEndpointsApiExplorer();
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
app.MapControllers();
app.UseAuthorization();

app.MapGet("/", () => (new { app = "payobills.bills" }));

app.MapGraphQL();

// var appUri = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();
// Console.WriteLine($"App running at {appUri.ToString()}");

app.Run();
