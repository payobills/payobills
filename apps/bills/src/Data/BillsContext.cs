using MongoDB.Driver;
using Payobills.Bills.Data.Contracts.Models;

namespace Payobills.Bills.Data.Contracts;

public class BillsContext : IBillsContext
{
    public BillsContext(IMongoClient mongoClient, string databaseName = DATABASE_NAME, string collectionName = COLLECTION_NAME)
    {
        MongoClient = mongoClient;
        DatabaseName = string.IsNullOrEmpty(databaseName) ? DATABASE_NAME : databaseName;
        CollectionName = string.IsNullOrEmpty(collectionName) ? COLLECTION_NAME : collectionName;
    }
    private string DatabaseName;
    private string CollectionName;

    private const string DATABASE_NAME = "payobills";
    private const string COLLECTION_NAME = "bills";

    public IMongoClient MongoClient { get; }
    public IMongoCollection<Bill> Bills => MongoClient.GetDatabase(DatabaseName).GetCollection<Bill>(CollectionName);
}
