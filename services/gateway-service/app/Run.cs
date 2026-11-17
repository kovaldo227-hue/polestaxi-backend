using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PolesTaxi.GatewayService.Config;
using PolesTaxi.Shared.Hosting;

namespace PolesTaxi.GatewayService.App;

public static class Run
{
    public static async Task<int> ExecuteAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Принудительно задаем порт
        builder.WebHost.UseUrls("http://0.0.0.0:8080");
        
        var app = builder.Build();
        
        // Два маршрута: корень и healthz
        app.MapGet("/", () => "Gateway is working!");
        app.MapGet("/healthz", () => "ok");

        Console.WriteLine("gateway ready");
        await app.RunAsync();
        return 0;
    }
}