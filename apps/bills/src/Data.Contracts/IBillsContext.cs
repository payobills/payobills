using MongoDB.Driver;
using Payobills.Bills.Data.Contracts.Models;

namespace Payobills.Bills.Data.Contracts;

public interface IBillsContext
{
    public IMongoClient MongoClient { get; }
    public IMongoCollection<Bill> Bills { get; }
}
