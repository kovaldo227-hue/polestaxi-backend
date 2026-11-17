using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PolesTaxi.AuthService.App.Db.Redis;
using PolesTaxi.AuthService.Config;
using PolesTaxi.AuthService.Handler.Grpc;
using PolesTaxi.AuthService.Repository.Redis;
using PolesTaxi.Shared.Hosting;
using StackExchange.Redis;

namespace PolesTaxi.AuthService.App;

public static class Run
{
    public static async Task<int> ExecuteAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = AppConfig.FromEnvironment();

        builder.WebHost.ConfigureKestrel(options =>
        {
            var httpPort = int.Parse(config.HttpAddr.Split(':').Last());
            var grpcPort = int.Parse(config.GrpcAddr.Split(':').Last());
            Listen.DualPorts(options, httpPort, grpcPort);
        });

        builder.Services.AddPolesTaxiGrpc();
        builder.Services.AddSingleton<GrpcHandler>();

        var redis = await RedisClient.New(config.RedisConnection, CancellationToken.None);
        builder.Services.AddSingleton(redis);
        
        // Вот эта строчка нужна для работы DI
        builder.Services.AddSingleton<IDatabase>(sp => 
            sp.GetRequiredService<RedisClient>().Database);
            
        builder.Services.AddSingleton<AuthRedisRepository>();

        var app = builder.Build();
        app.MapGrpcService<GrpcHandler>();
        app.MapHealth();

        Console.WriteLine("auth-service ready");
        await app.RunAsync();
        return 0;
    }
}