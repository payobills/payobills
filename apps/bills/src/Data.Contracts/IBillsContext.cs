using MongoDB.Driver;
using Payobills.Bills.Models;

namespace Payobills.Contracts.Bills.Data;

public interface IBillsContext
{
    private const string DB_NAME = "payobills";
    private const string COLLECTION_NAME = "payobills";

    public IMongoClient MongoClient { get; set; }
    public IMongoCollection<Bill> Bills => MongoClient.GetDatabase(DB_NAME).GetCollection<Bill>(COLLECTION_NAME);
}
