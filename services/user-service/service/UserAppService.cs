using PolesTaxi.Shared.Types;
using PolesTaxi.UserService.Entity.Service;
using PolesTaxi.UserService.Repository.Mongo;
namespace PolesTaxi.UserService.Service;
public sealed class UserAppService
{
    private readonly UserMongoRepository _users;
    public UserAppService(UserMongoRepository users) { _users = users; }
    public async Task<User> Register(string name, string phone, string email, string password, Role role, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var user = new User { Id = string.Empty, Name = name, Phone = phone, Email = email, PasswordHash = password,
            Roles = new List<Role> { role }, Rating = 0, RecentRatings = new List<double>(), IsDeleted = false, CreatedAt = now, UpdatedAt = now };
        return await _users.Create(user, ct);
    }
}
