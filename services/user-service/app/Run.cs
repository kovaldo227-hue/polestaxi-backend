using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PolesTaxi.Shared.Hosting;
using PolesTaxi.UserService.App.Db.Mongo;
using PolesTaxi.UserService.Config;
using PolesTaxi.UserService.Entity.Repository;
using PolesTaxi.UserService.Handler.Grpc;
using PolesTaxi.UserService.Repository.Mongo;

namespace PolesTaxi.UserService.App;

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
        
        // Явно указываем полный путь к нашему классу, чтобы не было конфликта
        var mongo = await PolesTaxi.UserService.App.Db.Mongo.MongoClient.New(
            config.MongoConnection, config.MongoDatabase, CancellationToken.None);
        
        builder.Services.AddSingleton(mongo);
        
        builder.Services.AddSingleton<IMongoCollection<UserDocument>>(sp => 
            sp.GetRequiredService<PolesTaxi.UserService.App.Db.Mongo.MongoClient>().Users);
        builder.Services.AddSingleton<UserMongoRepository>();

        var app = builder.Build();
        app.MapGrpcService<GrpcHandler>();
        app.MapHealth();

        Console.WriteLine("user-service ready");
        await app.RunAsync();
        return 0;
    }
}