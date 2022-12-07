using payobills.bills.svc;
using payobills.bills.repos;
using payobills.bills.data;
using Microsoft.EntityFrameworkCore;
using payobills.bills.gql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
builder.Services.AddGraphQLServer().AddQueryType<Query>().ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

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

// app.MapControllers();

app.MapGet("/", () => (new { app = "payobills.bills" }));

app.MapGraphQL();

app.Run();
