using FluentAssertions;
using PolesTaxi.Shared.Types;
using PolesTaxi.UserService.App.Db.Mongo;
using PolesTaxi.UserService.Entity.Service;
using PolesTaxi.UserService.Repository.Mongo;
using Testcontainers.MongoDb;
using Xunit;

namespace PolesTaxi.UserService.Tests;

public sealed class MongoDbFixture : IAsyncLifetime
{
    public MongoDbContainer Container { get; } = new MongoDbBuilder().WithImage("mongo:7").Build();

    public Task InitializeAsync() => Container.StartAsync();
    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}

public sealed class UserMongoRepositoryTests : IClassFixture<MongoDbFixture>
{
    private readonly MongoDbFixture _fx;
    private int _dbCounter;

    public UserMongoRepositoryTests(MongoDbFixture fx) => _fx = fx;

    private async Task<UserMongoRepository> Repo()
    {
        _dbCounter++;
        var mongo = await MongoClient.New(_fx.Container.GetConnectionString(), $"users_{_dbCounter}", CancellationToken.None);
        return new UserMongoRepository(mongo.Users);
    }

    private static User NewUser(string email) => new()
    {
        Id = string.Empty,
        Name = "Test",
        Phone = "+70000000000",
        Email = email,
        PasswordHash = "hash",
        Roles = new List<Role> { Role.User },
        Rating = 0,
        RecentRatings = new List<double>(),
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Create_and_get_by_email()
    {
        var repo = await Repo();
        await repo.Create(NewUser("test@example.com"), CancellationToken.None);
        
        var loaded = await repo.GetByEmail("test@example.com", CancellationToken.None);
        loaded.Should().NotBeNull();
        loaded.Email.Should().Be("test@example.com");
    }
}
