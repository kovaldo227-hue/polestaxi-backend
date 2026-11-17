using System.Text.Json;
using PolesTaxi.AuthService.Entity.Repository;
using PolesTaxi.AuthService.Entity.Service;
using PolesTaxi.Shared.Types;
using StackExchange.Redis;

namespace PolesTaxi.AuthService.Repository.Redis;

public sealed class AuthRedisRepository
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IDatabase _db;

    public AuthRedisRepository(IDatabase db)
    {
        _db = db;
    }

    public Task SetAccess(TokenRecord token, TimeSpan ttl, CancellationToken ct) => Set("access", token, ttl, ct);
    public Task<TokenRecord?> GetAccess(string jti, CancellationToken ct) => Get("access", jti, ct);
    public Task DeleteAccess(string jti, CancellationToken ct) => Delete("access", jti, ct);
    public Task SetRefresh(TokenRecord token, TimeSpan ttl, CancellationToken ct) => Set("refresh", token, ttl, ct);
    public Task<TokenRecord?> GetRefresh(string jti, CancellationToken ct) => Get("refresh", jti, ct);
    public Task DeleteRefresh(string jti, CancellationToken ct) => Delete("refresh", jti, ct);

    public async Task DeletePair(string accessJti, string refreshJti, CancellationToken ct)
    {
        await DeleteAccess(accessJti, ct);
        await DeleteRefresh(refreshJti, ct);
    }

    private async Task Set(string kind, TokenRecord token, TimeSpan ttl, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var doc = new TokenDocument { UserId = token.UserId, Roles = token.Roles.Select(r => r.ToWire()).ToList(), Family = token.Family };
        await _db.StringSetAsync($"{kind}:{token.Jti}", JsonSerializer.Serialize(doc, Json), ttl);
    }

    private async Task<TokenRecord?> Get(string kind, string jti, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var value = await _db.StringGetAsync($"{kind}:{jti}");
        if (value.IsNullOrEmpty) return null;
        var doc = JsonSerializer.Deserialize<TokenDocument>(value!, Json);
        if (doc is null) return null;
        return new TokenRecord { Jti = jti, UserId = doc.UserId, Roles = doc.Roles.Select(EnumMaps.RoleFromWire).ToList(), Family = doc.Family };
    }

    private async Task Delete(string kind, string jti, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await _db.KeyDeleteAsync($"{kind}:{jti}");
    }
}