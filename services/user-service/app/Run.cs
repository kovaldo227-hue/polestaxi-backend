using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PolesTaxi.UserService.app.db.mongo;
using PolesTaxi.UserService.repository.mongo;
using PolesTaxi.UserService.service;

namespace PolesTaxi.UserService.app;

public static class Run
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<MongoDbContext>();
        builder.Services.AddScoped<IUserRepository, UserMongoRepository>();
        builder.Services.AddScoped<IUserAppService, UserAppService>();
        
        // gRPC временно отключаем, чтобы не было ошибок (пока нет .proto)
        // builder.Services.AddGrpc(); 
    }

    public static void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.MapControllers();
        // app.MapGrpcService<GrpcHandler>(); 
    }
}