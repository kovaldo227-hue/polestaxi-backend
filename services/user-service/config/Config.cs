namespace PolesTaxi.UserService.Config;

public sealed class AppConfig
{
    public string HttpAddr { get; init; } = "http://0.0.0.0:8081";
    public string GrpcAddr { get; init; } = "http://0.0.0.0:9081";
    public string MongoConnection { get; init; } = "mongodb://127.0.0.1:27018";
    public string MongoDatabase { get; init; } = "polestaxi_users";

    public static AppConfig FromEnvironment()
    {
        return new AppConfig
        {
            HttpAddr = Environment.GetEnvironmentVariable("HTTP_ADDR") ?? "http://0.0.0.0:8081",
            GrpcAddr = Environment.GetEnvironmentVariable("GRPC_ADDR") ?? "http://0.0.0.0:9081",
            MongoConnection = Environment.GetEnvironmentVariable("MONGO_CONNECTION") ?? "mongodb://127.0.0.1:27018",
            MongoDatabase = Environment.GetEnvironmentVariable("MONGO_USER_DATABASE") ?? "polestaxi_users"
        };
    }
}