namespace PolesTaxi.AuthService.Config;

public sealed class AppConfig
{
    public string RedisConnection { get; init; } = "127.0.0.1:6379";

    public static AppConfig FromEnvironment()
    {
        return new AppConfig
        {
            RedisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "127.0.0.1:6379"
        };
    }
}