using MongoDB.Bson;
using MongoDB.Driver;
using PolesTaxi.Shared.Errors;
using PolesTaxi.Shared.Types;
using PolesTaxi.UserService.Entity.Repository;
using PolesTaxi.UserService.Entity.Service;
namespace PolesTaxi.UserService.Repository.Mongo;
public sealed class UserMongoRepository
{
    private readonly IMongoCollection<UserDocument> _users;
    public UserMongoRepository(IMongoCollection<UserDocument> users) { _users = users; }
    public async Task<User> Create(User user, CancellationToken cancellationToken)
    {
        var doc = ToDocument(user);
        try { await _users.InsertOneAsync(doc, cancellationToken: cancellationToken); }
        catch (MongoWriteException ex) when (ex.WriteError.Code == 11000) { throw AppException.Conflict("email or phone already exists"); }
        return ToDomain(doc);
    }
    public async Task SoftDelete(string id, CancellationToken cancellationToken)
    {
        await _users.UpdateOneAsync(x => x.Id == id, Builders<UserDocument>.Update.Set(x => x.IsDeleted, true), cancellationToken: cancellationToken);
    }
    private static UserDocument ToDocument(User user) => new()
    {
        Id = string.IsNullOrEmpty(user.Id) ? ObjectId.GenerateNewId().ToString() : user.Id,
        Name = user.Name, Phone = user.Phone, Email = user.Email.ToLowerInvariant(),
        PasswordHash = user.PasswordHash, Roles = user.Roles.Select(r => r.ToWire()).ToList(),
        Rating = user.Rating, RecentRatings = user.RecentRatings.ToList(),
        IsDeleted = user.IsDeleted, CreatedAt = user.CreatedAt, UpdatedAt = user.UpdatedAt
    };
    private static User ToDomain(UserDocument doc) => new()
    {
        Id = doc.Id, Name = doc.Name, Phone = doc.Phone, Email = doc.Email,
        PasswordHash = doc.PasswordHash, Roles = doc.Roles.Select(EnumMaps.RoleFromWire).ToList(),
        Rating = doc.Rating, RecentRatings = doc.RecentRatings, IsDeleted = doc.IsDeleted,
        CreatedAt = doc.CreatedAt, UpdatedAt = doc.UpdatedAt
    };
}
