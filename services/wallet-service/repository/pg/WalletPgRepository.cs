using Dapper;
using Npgsql;
using PolesTaxi.Shared.Errors;
using PolesTaxi.WalletService.Entity.Service;

namespace PolesTaxi.WalletService.Repository.Pg;

public sealed class WalletPgRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public WalletPgRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Wallet> CreateWallet(string userId, string currency, CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            var wallet = await conn.QuerySingleAsync<Wallet>(
                "INSERT INTO wallets (id, user_id, balance, currency) VALUES (@Id, @UserId, 0, @Currency) RETURNING id AS Id, user_id AS UserId, balance AS Balance, currency AS Currency, created_at AS CreatedAt",
                new { Id = Guid.NewGuid(), UserId = userId, Currency = currency });
            return wallet;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw AppException.Conflict("wallet already exists");
        }
    }
}