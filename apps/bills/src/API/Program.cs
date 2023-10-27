using payobills.bills.svc;
using payobills.bills.repos;
using payobills.bills.data;
using Microsoft.EntityFrameworkCore;
using payobills.bills.gql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
  var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
  if (!string.IsNullOrEmpty(allowedOrigins))
  {
    options.AddPolicy(name: "allowedOrigins", policy
        => policy.WithOrigins(allowedOrigins.Split(",")));
  }
});

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
  using (var billsContext = serviceScope.ServiceProvider.GetService<ibillscontext>())
  {
    billsContext?.Database.EnsureCreated();
    billsContext?.SaveChanges();
  }
}

app.UseCors(corsPolicyName);
app.UseAuthorization();

app.MapGet("/", () => (new { app = "payobills.bills" }));
app.MapGraphQL();

app.Run();
