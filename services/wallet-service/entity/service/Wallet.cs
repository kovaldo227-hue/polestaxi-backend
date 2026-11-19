namespace PolesTaxi.WalletService.Entity.Service;

public enum WalletTxType { Unspecified = 0, Debit, Credit, Refund }
public enum WalletTxStatus { Unspecified = 0, Pending, Completed, Failed, RolledBack }

public sealed class Wallet
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public decimal Balance { get; init; }
    public required string Currency { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class WalletTransaction
{
    public required string Id { get; init; }
    public required string WalletId { get; init; }
    public WalletTxType Type { get; init; }
    public decimal Amount { get; init; }
    public WalletTxStatus Status { get; init; }
    public string? OrderId { get; init; }
    public string? IdempotencyKey { get; init; }
    public DateTime CreatedAt { get; init; }
}