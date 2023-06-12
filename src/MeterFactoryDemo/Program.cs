namespace MeterFactoryDemo;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton<IHostedService, HttpServerMetricsPublisher>();
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}