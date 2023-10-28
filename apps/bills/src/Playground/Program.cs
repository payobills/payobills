using Payobills.Bills.Data.Contracts;
using Payobills.Bills.Data;
using MongoDB.Driver;
using Payobills.Bills.Data.Contracts.Models;
using System.Text.Json;

var mongoClient = new MongoClient("mongodb://localhost:27017");
var billsContext = new BillsContext(mongoClient);
var billsRepo = new BillsRepo(billsContext);

await billsRepo.AddBillAsync(new Bill {
    Id = Guid.NewGuid(),
    Name = "AMEX",
    BillingDate = 3,
    CreatedAt=DateTime.UtcNow,
    UpdatedAt=DateTime.UtcNow,
    LatePayByDate = 23,
    PayByDate = 23
});

var bills = billsRepo.GetBillsAsync();

Console.WriteLine(JsonSerializer.Serialize(bills.ToList(), new JsonSerializerOptions {
    WriteIndented = true
}));