using ClickHouse.Client.ADO;

namespace PolesTaxi.AnalyticService.App.Db.ClickHouse;

public sealed class ClickHouseStore
{
    public string ConnectionString { get; }

    private ClickHouseStore(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public static async Task<ClickHouseStore> New(string connectionString, CancellationToken cancellationToken)
    {
        var store = new ClickHouseStore(connectionString);
        await store.EnsureTables(cancellationToken);
        return store;
    }

    private async Task EnsureTables(CancellationToken cancellationToken)
    {
        await using var conn = new ClickHouseConnection(ConnectionString);
        await conn.OpenAsync(cancellationToken);

        // 1. Таблица user_registered
        await using (var cmd1 = conn.CreateCommand())
        {
            cmd1.CommandText = @"
                CREATE TABLE IF NOT EXISTS user_registered (
                    user_id String,
                    role LowCardinality(String),
                    event_time DateTime64(3, 'UTC')
                ) ENGINE = MergeTree ORDER BY (event_time, user_id);";
            await cmd1.ExecuteNonQueryAsync(cancellationToken);
        }

        // 2. Таблица order_completed
        await using (var cmd2 = conn.CreateCommand())
        {
            cmd2.CommandText = @"
                CREATE TABLE IF NOT EXISTS order_completed (
                    order_id String,
                    user_id String,
                    driver_id String,
                    taxi_type LowCardinality(String),
                    price Decimal(12, 2),
                    distance Int64,
                    event_time DateTime64(3, 'UTC')
                ) ENGINE = MergeTree ORDER BY (event_time, order_id);";
            await cmd2.ExecuteNonQueryAsync(cancellationToken);
        }

        // 3. Таблица trip_rated
        await using (var cmd3 = conn.CreateCommand())
        {
            cmd3.CommandText = @"
                CREATE TABLE IF NOT EXISTS trip_rated (
                    order_id String,
                    from_role LowCardinality(String),
                    score Int8,
                    event_time DateTime64(3, 'UTC')
                ) ENGINE = MergeTree ORDER BY (event_time, order_id);";
            await cmd3.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}