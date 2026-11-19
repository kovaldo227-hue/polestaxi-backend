using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace PolesTaxi.UserService.app.db.mongo;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        // Берем строку подключения из appsettings.json
        var connectionString = configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("PolesTaxiUsers");
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}