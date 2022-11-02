using payobills.bills.svc;
using payobills.bills.repos;
using payobills.bills.data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
    using (var billsContext = serviceScope.ServiceProvider.GetService<BillsContext>())
    {
        billsContext?.Database.EnsureCreated();
        billsContext?.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => (new { app = "payobills.bills" }));

app.Run();
