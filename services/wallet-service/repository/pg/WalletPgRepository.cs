using Dapper;
using Npgsql;
using PolesTaxi.WalletService.Errors;
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

            await ApplyMoney(userId, WalletTxType.Credit, 5000m, null, $"signup:{userId}", cancellationToken);

            return await GetByUserId(userId, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw AppException.Conflict("wallet already exists");
        }
    }

    public async Task<Wallet> GetByUserId(string userId, CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        var wallet = await conn.QuerySingleOrDefaultAsync<Wallet>(
            "SELECT id AS Id, user_id AS UserId, balance AS Balance, currency AS Currency, created_at AS CreatedAt FROM wallets WHERE user_id = @userId",
            new { userId });

        if (wallet is null) throw AppException.NotFound("wallet not found");
        return wallet;
    }

    public async Task<WalletTransaction> ApplyMoney(string userId, WalletTxType type, decimal amount, string? orderId, string? idempotencyKey, CancellationToken cancellationToken)
    {
        if (amount <= 0) throw AppException.Validation("amount must be positive");

        var key = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        if (key is not null)
        {
            var existing = await conn.QuerySingleOrDefaultAsync<WalletTransaction>(
                "SELECT id AS Id, wallet_id AS WalletId, type AS Type, amount AS Amount, status AS Status, order_id AS OrderId, idempotency_key AS IdempotencyKey, created_at AS CreatedAt FROM transactions WHERE idempotency_key = @key",
                new { key });

            if (existing is not null) return existing;
        }

        await using var tx = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            var wallet = await conn.QuerySingleOrDefaultAsync<Wallet>(
                "SELECT id AS Id, user_id AS UserId, balance AS Balance, currency AS Currency, created_at AS CreatedAt FROM wallets WHERE user_id = @userId FOR UPDATE",
                new { userId }, tx);

            if (wallet is null) throw AppException.NotFound("wallet not found");

            var delta = type == WalletTxType.Debit ? -amount : amount;
            var newBalance = wallet.Balance + delta;
            if (newBalance < 0) throw AppException.Conflict("insufficient funds");

            await conn.ExecuteAsync("UPDATE wallets SET balance = @balance WHERE id = @id", new { balance = newBalance, id = wallet.Id }, tx);

            var row = await conn.QuerySingleAsync<WalletTransaction>(
                @"INSERT INTO transactions (id, wallet_id, type, amount, status, order_id, idempotency_key) 
                  VALUES (@Id, @WalletId, @Type, @Amount, 'completed', @OrderId, @IdempotencyKey) 
                  RETURNING id AS Id, wallet_id AS WalletId, type AS Type, amount AS Amount, status AS Status, order_id AS OrderId, idempotency_key AS IdempotencyKey, created_at AS CreatedAt",
                new { Id = Guid.NewGuid(), WalletId = wallet.Id, Type = type.ToString().ToLower(), Amount = amount, OrderId = orderId, IdempotencyKey = key }, tx);

            await tx.CommitAsync(cancellationToken);
            return row;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}