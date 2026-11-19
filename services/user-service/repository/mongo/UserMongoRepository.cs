using MongoDB.Driver;
using PolesTaxi.UserService.app.db.mongo; // Проверьте, что тут .db.mongo
using PolesTaxi.UserService.entity;      // <--- Ссылка на User

namespace PolesTaxi.UserService.repository.mongo;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task CreateAsync(User user);
}

public class UserMongoRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserMongoRepository(MongoDbContext context)
    {
        _users = context.GetCollection<User>("Users");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User user)
    {
        await _users.InsertOneAsync(user);
    }
}